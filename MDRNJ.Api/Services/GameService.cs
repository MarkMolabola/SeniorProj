using AutoMapper;
using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    public class GameService : IGameService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public GameService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<GameStatsDto> GetStatsAsync(Guid userId)
        {
            var stats = await _db.GameStats.FirstOrDefaultAsync(g => g.UserId == userId);
            if (stats is null)
            {
                stats = new GameStatsEntity { UserId = userId };
                _db.GameStats.Add(stats);
                await _db.SaveChangesAsync();
            }

            var dto = _mapper.Map<GameStatsDto>(stats);
            dto.XPToNextLevel = XPForNextLevel(stats.CurrentLevel) - stats.TotalXP;
            return dto;
        }

        public async Task<GameStatsDto> AwardXPAsync(Guid userId, int amount, string reason)
        {
            var stats = await _db.GameStats.FirstOrDefaultAsync(g => g.UserId == userId);
            if (stats is null)
            {
                stats = new GameStatsEntity { UserId = userId };
                _db.GameStats.Add(stats);
            }

            stats.TotalXP += amount;
            stats.CurrentLevel = LevelFromXP(stats.TotalXP);
            stats.TotalWorkouts++;

            var today = DateTime.UtcNow.Date;
            if (stats.LastWorkoutDate?.Date == today.AddDays(-1))
            {
                stats.CurrentStreak++;
                if (stats.CurrentStreak > stats.LongestStreak)
                    stats.LongestStreak = stats.CurrentStreak;
            }
            else if (stats.LastWorkoutDate?.Date != today)
            {
                stats.CurrentStreak = 1;
            }
            stats.LastWorkoutDate = DateTime.UtcNow;

            if (Enum.TryParse<XPReason>(reason, out var xpReason))
            {
                _db.XPTransactions.Add(new XPTransactionEntity
                {
                    UserId = userId,
                    Amount = amount,
                    Reason = xpReason
                });
            }

            await _db.SaveChangesAsync();

            var dto = _mapper.Map<GameStatsDto>(stats);
            dto.XPToNextLevel = XPForNextLevel(stats.CurrentLevel) - stats.TotalXP;
            return dto;
        }

        public async Task<List<XPTransactionDto>> GetXPHistoryAsync(Guid userId)
        {
            var transactions = await _db.XPTransactions
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<XPTransactionDto>>(transactions);
        }

        private static int LevelFromXP(int xp)
        {
            int level = 1;
            while (xp >= XPForNextLevel(level)) level++;
            return level;
        }

        private static int XPForNextLevel(int level) => level * level * 100;
    }
}
