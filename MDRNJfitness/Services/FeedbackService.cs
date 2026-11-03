using MDRNJ.Shared.DTOs.Feedback;
using MDRNJfitness.Helpers.Extensions;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly HttpClient _http;

        public FeedbackService(HttpClient http) => _http = http;

        public Task SubmitAsync(SubmitFeedbackDto dto) =>
            _http.PostAsync("api/feedback", dto);

        public async Task<List<FeedbackDto>> GetInboxAsync(bool includeResolved) =>
            await _http.GetAsync<List<FeedbackDto>>($"api/feedback?includeResolved={includeResolved}") ?? new();

        public Task ResolveAsync(Guid feedbackId) =>
            _http.PostAsync($"api/feedback/{feedbackId}/resolve", new { });

        public Task ReopenAsync(Guid feedbackId) =>
            _http.PostAsync($"api/feedback/{feedbackId}/reopen", new { });
    }
}
