using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    /// <summary>
    /// Passive dungeon gold. Nothing runs in the background: gold is worked out from
    /// the time since the last collect and the hero's current power (DungeonRules).
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
            return await BuildAsync(userId, stats, DateTime.UtcNow);
        }

        public async Task<DungeonCollectDto> CollectAsync(Guid userId)
        {
            var stats = await GetOrCreateStatsAsync(userId);
            var now = DateTime.UtcNow;
            var power = await PowerAsync(userId);

            var collected = DungeonRules.PendingGold(power, now - stats.DungeonCollectedAt);
            _goals.GrantGold(stats, collected, GoldReason.Dungeon, $"Dungeon run vs {DungeonRules.MonsterFor(power).Name}");
            stats.DungeonCollectedAt = now;
            await _db.SaveChangesAsync();

            return new DungeonCollectDto
            {
                Collected = collected,
                Gold = stats.Gold,
                Dungeon = await BuildAsync(userId, stats, now)
            };
        }

        private async Task<DungeonDto> BuildAsync(Guid userId, GameStatsEntity stats, DateTime now)
        {
            var power = await PowerAsync(userId);
            var elapsed = now - stats.DungeonCollectedAt;
            var monster = DungeonRules.MonsterFor(power);

            return new DungeonDto
            {
                Power = power,
                GoldPerHour = DungeonRules.GoldPerHour(power),
                PendingGold = DungeonRules.PendingGold(power, elapsed),
                CapGold = DungeonRules.CapGold(power),
                CapHours = DungeonRules.CapHours,
                SecondsAccrued = Math.Clamp(elapsed.TotalSeconds, 0, DungeonRules.CapHours * 3600),
                MonsterId = monster.Id,
                MonsterName = monster.Name
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
