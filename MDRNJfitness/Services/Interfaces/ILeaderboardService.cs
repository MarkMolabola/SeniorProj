using MDRNJ.Shared.DTOs.Leaderboard;

namespace MDRNJfitness.Services.Interfaces
{
    public interface ILeaderboardService
    {
        Task<List<LeaderboardEntryDto>> GetTopAsync(int count = 50);
    }
}
