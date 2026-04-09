using MDRNJ.Shared.DTOs.Leaderboard;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface ILeaderboardService
    {
        Task<List<LeaderboardEntryDto>> GetTopAsync(int count);
        Task<LeaderboardEntryDto> GetUserRankAsync(Guid userId);
    }
}
