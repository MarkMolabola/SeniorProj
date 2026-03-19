using MDRNJ.Shared.DTOs.Game;
using MDRNJfitness.Helpers.Extensions;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Services
{
    public class GameService : IGameService
    {
        private readonly HttpClient _http;

        public GameService(HttpClient http) => _http = http;

        public Task<GameStatsDto> GetStatsAsync() =>
            _http.GetAsync<GameStatsDto>("api/game/stats")!;

        public Task<List<AchievementDto>> GetAchievementsAsync() =>
            _http.GetAsync<List<AchievementDto>>("api/achievements")!;
    }
}
