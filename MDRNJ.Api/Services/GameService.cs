using AutoMapper;
using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;
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
            dto.UnlimitedGold = await _db.Users.Where(u => u.UserId == userId).Select(u => u.IsAdmin).FirstOrDefaultAsync();

            // Streak is stale if the last workout was 2+ days ago (missed yesterday)
            var today = DateTime.UtcNow.Date;
            if (stats.LastWorkoutDate.HasValue && stats.LastWorkoutDate.Value.Date < today.AddDays(-1))
            {
                dto.CurrentStreak = 0;
                stats.CurrentStreak = 0;
                await _db.SaveChangesAsync();
            }

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

        public async Task<List<GoldTransactionDto>> GetGoldHistoryAsync(Guid userId, int count)
        {
            var transactions = await _db.GoldTransactions
                .Where(g => g.UserId == userId)
                .OrderByDescending(g => g.CreatedAt)
                .Take(Math.Clamp(count, 1, 200))
                .ToListAsync();

            return _mapper.Map<List<GoldTransactionDto>>(transactions);
        }

        private static int LevelFromXP(int xp) => AccountProgression.LevelFor(xp);

        private static int XPForNextLevel(int level) => AccountProgression.XPForNextLevel(level);
    }
}
