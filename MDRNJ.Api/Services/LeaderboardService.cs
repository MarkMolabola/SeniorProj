using MDRNJ.Api.Data;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Character;
using MDRNJ.Shared.DTOs.Leaderboard;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly AppDbContext _db;

        public LeaderboardService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<LeaderboardEntryDto>> GetTopAsync(int count)
        {
            var entries = await _db.GameStats
                .Include(g => g.User)
                    .ThenInclude(u => u.Profile)
                .Include(g => g.User)
                    .ThenInclude(u => u.CharacterState)
                .OrderByDescending(g => g.TotalXP)
                .Take(count)
                .ToListAsync();

            return entries.Select((g, i) => new LeaderboardEntryDto
            {
                Rank = i + 1,
                UserId = g.UserId,
                Username = g.User.Username,
                DisplayName = g.User.Profile?.DisplayName ?? g.User.Username,
                CurrentLevel = g.CurrentLevel,
                TotalXP = g.TotalXP,
                CurrentStreak = g.CurrentStreak,
                CharacterSprite = g.User.CharacterState is not null
                    ? new SpriteConfigDto
                    {
                        BodySpritePath = $"sprites/body/{g.User.CharacterState.BodyType}",
                        OutfitSpritePath = $"sprites/outfit/{g.User.CharacterState.OutfitSlot}",
                        EquipmentSpritePath = $"sprites/equipment/{g.User.CharacterState.EquipmentSlot}",
                        AccessorySpritePath = g.User.CharacterState.AccessorySlot is not null
                            ? $"sprites/accessory/{g.User.CharacterState.AccessorySlot}"
                            : null
                    }
                    : new SpriteConfigDto
                    {
                        BodySpritePath = "sprites/body/default",
                        OutfitSpritePath = "sprites/outfit/default",
                        EquipmentSpritePath = "sprites/equipment/default"
                    }
            }).ToList();
        }

        public async Task<LeaderboardEntryDto> GetUserRankAsync(Guid userId)
        {
            var allStats = await _db.GameStats
                .Include(g => g.User)
                    .ThenInclude(u => u.Profile)
                .Include(g => g.User)
                    .ThenInclude(u => u.CharacterState)
                .OrderByDescending(g => g.TotalXP)
                .ToListAsync();

            var index = allStats.FindIndex(g => g.UserId == userId);
            if (index < 0) throw new KeyNotFoundException("User not found on leaderboard.");

            var g = allStats[index];
            return new LeaderboardEntryDto
            {
                Rank = index + 1,
                UserId = g.UserId,
                Username = g.User.Username,
                DisplayName = g.User.Profile?.DisplayName ?? g.User.Username,
                CurrentLevel = g.CurrentLevel,
                TotalXP = g.TotalXP,
                CurrentStreak = g.CurrentStreak,
                CharacterSprite = g.User.CharacterState is not null
                    ? new SpriteConfigDto
                    {
                        BodySpritePath = $"sprites/body/{g.User.CharacterState.BodyType}",
                        OutfitSpritePath = $"sprites/outfit/{g.User.CharacterState.OutfitSlot}",
                        EquipmentSpritePath = $"sprites/equipment/{g.User.CharacterState.EquipmentSlot}",
                        AccessorySpritePath = g.User.CharacterState.AccessorySlot is not null
                            ? $"sprites/accessory/{g.User.CharacterState.AccessorySlot}"
                            : null
                    }
                    : new SpriteConfigDto
                    {
                        BodySpritePath = "sprites/body/default",
                        OutfitSpritePath = "sprites/outfit/default",
                        EquipmentSpritePath = "sprites/equipment/default"
                    }
            };
        }
    }
}
