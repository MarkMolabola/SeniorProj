using MDRNJ.Shared.DTOs.Leaderboard;
using MDRNJfitness.Helpers.Extensions;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly HttpClient _http;

        public LeaderboardService(HttpClient http) => _http = http;

        public Task<List<LeaderboardEntryDto>> GetTopAsync(int count = 50) =>
            _http.GetAsync<List<LeaderboardEntryDto>>($"api/leaderboard?count={count}")!;
    }
}
