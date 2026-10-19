using MDRNJ.Shared.DTOs.Character;
using MDRNJfitness.Helpers.Extensions;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Services
{
    public class CharacterService : ICharacterService
    {
        private readonly HttpClient _http;

        public CharacterService(HttpClient http) => _http = http;

        public Task<CharacterStateDto> GetCharacterAsync() =>
            _http.GetAsync<CharacterStateDto>("api/character")!;

        public Task<CharacterStateDto> SetAvatarAsync(string avatarId) =>
            _http.PutAsync<CharacterStateDto>("api/character/avatar", new SetAvatarDto { AvatarId = avatarId })!;

        public Task<ActivityScoreDto> GetActivityScoreAsync() =>
            _http.GetAsync<ActivityScoreDto>("api/character/activity-score")!;

        public Task<ActivityScoreDto> RecalculateActivityScoreAsync() =>
            _http.PostAsync<ActivityScoreDto>("api/character/activity-score/recalculate", new { })!;

        public Task<ActivityScoreDto> SpendStatPointsAsync(string stat, int points = 1) =>
            _http.PostAsync<ActivityScoreDto>("api/character/stat-points", new SpendStatPointsDto { Stat = stat, Points = points })!;
    }
}
