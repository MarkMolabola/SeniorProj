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

        public Task<ActivityScoreDto> GetActivityScoreAsync() =>
            _http.GetAsync<ActivityScoreDto>("api/character/activity-score")!;

        public Task<ActivityScoreDto> RecalculateActivityScoreAsync() =>
            _http.PostAsync<ActivityScoreDto>("api/character/activity-score/recalculate", new { })!;
    }
}
