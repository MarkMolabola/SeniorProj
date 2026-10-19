using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    /// <summary>
    /// Passive dungeon. Nothing runs in the background: gold and kills are worked out from
    /// the time since the last collect and the hero's current power (DungeonRules). Each
    /// collect banks the kills: they clear floors, damage the weekly boss, and can drop
    /// dungeon-only gear.
    /// </summary>
    public class DungeonService : IDungeonService
    {
        private readonly AppDbContext _db;
        private readonly IGoalProgressService _goals;

        public DungeonService(AppDbContext db, IGoalProgressService goals)
        {
            _db = db;
            _goals = goals;
        }

        public async Task<DungeonDto> GetAsync(Guid userId)
        {
            var stats = await GetOrCreateStatsAsync(userId);
            var now = DateTime.UtcNow;
            if (ResetBossIfNewWeek(stats, now))
                await _db.SaveChangesAsync();
            return Build(stats, await PowerAsync(userId), now);
        }

        public async Task<DungeonCollectDto> CollectAsync(Guid userId)
        {
            var stats = await GetOrCreateStatsAsync(userId);
            var now = DateTime.UtcNow;
            var power = await PowerAsync(userId);
            var elapsed = now - stats.DungeonCollectedAt;
            var floor = DungeonRules.Floor(stats.DungeonFloor);

            var result = new DungeonCollectDto
            {
                Collected = DungeonRules.PendingGold(power, elapsed, floor.Number),
                Kills = DungeonRules.PendingKills(power, elapsed)
            };
            _goals.GrantGold(stats, result.Collected, GoldReason.Dungeon, $"Dungeon floor {floor.Number} vs {floor.MonsterName}s");
            stats.DungeonCollectedAt = now;

            // Floors: bank the kills, then go as deep as they and the hero's power allow
            var floorBefore = stats.DungeonFloor;
            AddFloorKills(stats, result.Kills, power);
            if (stats.DungeonFloor != floorBefore)
                result.NewFloor = stats.DungeonFloor;

            // Weekly boss: the same kills wear it down, if the hero is strong enough to fight
            ResetBossIfNewWeek(stats, now);
            var boss = DungeonRules.BossFor(stats.DungeonBossWeek!.Value);
            if (!stats.DungeonBossDefeated && power >= boss.MinPower && result.Kills > 0)
            {
                result.BossDamage = Math.Min(result.Kills, boss.Hp - stats.DungeonBossDamage);
                stats.DungeonBossDamage += result.BossDamage;
                if (stats.DungeonBossDamage >= boss.Hp)
                {
                    stats.DungeonBossDefeated = result.BossDefeated = true;
                    // The boss always drops dungeon-only gear while there's any left to find
                    result.DroppedItemId = await GrantDropAsync(userId, $"Weekly boss: {boss.Name}");
                    result.BossGold = DungeonRules.BossGold + (result.DroppedItemId is null ? DungeonRules.BossGoldNoDrop : 0);
                    _goals.GrantGold(stats, result.BossGold, GoldReason.Boss, $"Weekly boss: {boss.Name}");
                }
            }

            // Rare drop from the run itself
            if (result.DroppedItemId is null
                && Random.Shared.NextDouble() < DungeonRules.DropChance(result.Kills, floor.Number))
                result.DroppedItemId = await GrantDropAsync(userId, $"Dungeon floor {floor.Number}");

            await _db.SaveChangesAsync();
            result.Gold = stats.Gold;
            result.Dungeon = Build(stats, power, now);
            return result;
        }

        /// <summary>Banks kills on the current floor and descends while the floor is cleared and the hero has the power.</summary>
        private static void AddFloorKills(GameStatsEntity stats, int kills, float power)
        {
            stats.DungeonFloor = Math.Clamp(stats.DungeonFloor, 1, DungeonRules.Floors.Count);
            stats.DungeonFloorKills += kills;

            while (!DungeonRules.IsBottom(stats.DungeonFloor))
            {
                var current = DungeonRules.Floor(stats.DungeonFloor);
                var next = DungeonRules.Floor(stats.DungeonFloor + 1);
                if (stats.DungeonFloorKills < current.KillsToClear || power < next.MinPower)
                    break;
                stats.DungeonFloorKills -= current.KillsToClear;
                stats.DungeonFloor = next.Number;
            }

            // Cleared but not strong enough to go deeper: hold at "cleared" instead of piling up kills
            if (!DungeonRules.IsBottom(stats.DungeonFloor))
                stats.DungeonFloorKills = Math.Min(stats.DungeonFloorKills, DungeonRules.Floor(stats.DungeonFloor).KillsToClear);
        }

        /// <summary>A new week brings a new boss at full health. True if anything changed.</summary>
        private static bool ResetBossIfNewWeek(GameStatsEntity stats, DateTime now)
        {
            var week = DungeonRules.WeekStart(now);
            if (stats.DungeonBossWeek == week) return false;
            stats.DungeonBossWeek = week;
            stats.DungeonBossDamage = 0;
            stats.DungeonBossDefeated = false;
            return true;
        }

        /// <summary>Gives a random dungeon-only item the player doesn't have yet; null when they own them all.</summary>
        private async Task<string?> GrantDropAsync(Guid userId, string source)
        {
            var owned = await _db.UserGear.Where(g => g.UserId == userId).Select(g => g.ItemId).ToListAsync();
            // Also skip anything already added in this request
            owned.AddRange(_db.ChangeTracker.Entries<UserGearEntity>()
                .Where(e => e.State == EntityState.Added && e.Entity.UserId == userId)
                .Select(e => e.Entity.ItemId));

            var candidates = GearCatalog.DungeonDrops.Where(i => !owned.Contains(i.Id)).ToList();
            if (candidates.Count == 0) return null;

            var item = candidates[Random.Shared.Next(candidates.Count)];
            _db.UserGear.Add(new UserGearEntity { UserId = userId, ItemId = item.Id, PricePaid = 0 });
            return item.Id;
        }

        private static DungeonDto Build(GameStatsEntity stats, float power, DateTime now)
        {
            var elapsed = now - stats.DungeonCollectedAt;
            var floor = DungeonRules.Floor(stats.DungeonFloor);
            var bottom = DungeonRules.IsBottom(floor.Number);
            var next = bottom ? null : DungeonRules.Floor(floor.Number + 1);
            var week = stats.DungeonBossWeek ?? DungeonRules.WeekStart(now);
            var boss = DungeonRules.BossFor(week);

            return new DungeonDto
            {
                Power = power,
                GoldPerHour = DungeonRules.GoldPerHour(power, floor.Number),
                PendingGold = DungeonRules.PendingGold(power, elapsed, floor.Number),
                CapGold = DungeonRules.CapGold(power, floor.Number),
                CapHours = DungeonRules.CapHours,
                SecondsAccrued = Math.Clamp(elapsed.TotalSeconds, 0, DungeonRules.CapHours * 3600),
                MonsterId = floor.MonsterId,
                MonsterName = floor.MonsterName,

                Floor = floor.Number,
                FloorCount = DungeonRules.Floors.Count,
                GoldMultiplier = floor.GoldMultiplier,
                KillsPerHour = DungeonRules.KillsPerHour(power),
                FloorKills = stats.DungeonFloorKills,
                KillsToClear = floor.KillsToClear,
                NextFloorMinPower = next is not null && power < next.MinPower ? next.MinPower : null,

                BossName = boss.Name,
                BossMonsterId = boss.MonsterId,
                BossHp = boss.Hp,
                BossDamage = stats.DungeonBossDamage,
                BossMinPower = boss.MinPower,
                BossDefeated = stats.DungeonBossDefeated,
                BossEndsAt = DateTime.SpecifyKind(week.AddDays(7), DateTimeKind.Utc)
            };
        }

        private async Task<float> PowerAsync(Guid userId)
        {
            var (str, end, flx) = await EffectiveStats.LoadAsync(_db, userId);
            return DungeonRules.Power(str, end, flx);
        }

        private async Task<GameStatsEntity> GetOrCreateStatsAsync(Guid userId)
        {
            var stats = await _db.GameStats.FirstOrDefaultAsync(g => g.UserId == userId);
            if (stats is not null) return stats;

            stats = new GameStatsEntity { UserId = userId };
            _db.GameStats.Add(stats);
            await _db.SaveChangesAsync();
            return stats;
        }
    }
}
