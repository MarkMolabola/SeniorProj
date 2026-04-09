using MDRNJ.Shared.DTOs.Challenge;
using MDRNJfitness.Helpers.Extensions;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Services
{
    public class ChallengeService : IChallengeService
    {
        private readonly HttpClient _http;

        public ChallengeService(HttpClient http) => _http = http;

        public Task<List<ChallengeDto>> GetActiveChallengesAsync() =>
            _http.GetAsync<List<ChallengeDto>>("api/challenges/active")!;

        public Task<List<ChallengeDto>> GetMyChallengesAsync() =>
            _http.GetAsync<List<ChallengeDto>>("api/challenges/my")!;

        public Task<ChallengeDto> CreateAsync(CreateChallengeDto dto) =>
            _http.PostAsync<ChallengeDto>("api/challenges", dto)!;

        public Task<ChallengeDto> JoinAsync(Guid challengeId) =>
            _http.PostAsync<ChallengeDto>($"api/challenges/{challengeId}/join", new { })!;
    }
}
