using MDRNJ.Shared.DTOs.Challenge;
using MDRNJfitness.Helpers.Extensions;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Services
{
    public class ChallengeService : IChallengeService
    {
        private readonly HttpClient _http;

        public ChallengeService(HttpClient http) => _http = http;

        public Task<List<ChallengeDto>> GetMyChallengesAsync() =>
            _http.GetAsync<List<ChallengeDto>>("api/challenges/my")!;

        public Task<ChallengeDto> CreateAsync(CreateChallengeDto dto) =>
            _http.PostAsync<ChallengeDto>("api/challenges", dto)!;

        public Task<ChallengeDto> AcceptAsync(Guid challengeId) =>
            _http.PostAsync<ChallengeDto>($"api/challenges/{challengeId}/accept", new { })!;

        public Task<ChallengeDto> DeclineAsync(Guid challengeId) =>
            _http.PostAsync<ChallengeDto>($"api/challenges/{challengeId}/decline", new { })!;

        public Task<ChallengeDto> CancelAsync(Guid challengeId) =>
            _http.PostAsync<ChallengeDto>($"api/challenges/{challengeId}/cancel", new { })!;

        public Task<ChallengeDto> SubmitAttemptAsync(Guid challengeId, float value) =>
            _http.PostAsync<ChallengeDto>($"api/challenges/{challengeId}/attempts", new SubmitAttemptDto { Value = value })!;

        public Task MarkResultSeenAsync(Guid challengeId) =>
            _http.PostAsync($"api/challenges/{challengeId}/seen", new { });
    }
}
