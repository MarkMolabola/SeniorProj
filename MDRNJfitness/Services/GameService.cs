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

        public Task<DungeonDto> GetDungeonAsync() =>
            _http.GetAsync<DungeonDto>("api/dungeon")!;

        public Task<DungeonCollectDto> CollectDungeonAsync() =>
            _http.PostAsync<DungeonCollectDto>("api/dungeon/collect", new { })!;

        public async Task CheckAchievementsAsync() =>
            await _http.PostAsync<List<AchievementDto>>("api/achievements/check", null);

        public Task<List<GoldTransactionDto>> GetGoldHistoryAsync(int count = 50) =>
            _http.GetAsync<List<GoldTransactionDto>>($"api/game/gold/history?count={count}")!;

        public Task<List<AchievementDto>> GetAchievementsAsync() =>
            _http.GetAsync<List<AchievementDto>>("api/achievements")!;
    }
}
