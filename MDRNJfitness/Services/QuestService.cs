using MDRNJ.Shared.DTOs.Quest;
using MDRNJfitness.Helpers.Extensions;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Services
{
    public class QuestService : IQuestService
    {
        private readonly HttpClient _http;

        public QuestService(HttpClient http) => _http = http;

        public Task<List<UserQuestDto>> GetMyQuestsAsync() =>
            _http.GetAsync<List<UserQuestDto>>("api/quests/my")!;

        public Task<List<QuestDto>> GetAllQuestsAsync() =>
            _http.GetAsync<List<QuestDto>>("api/quests")!;

        public Task<QuestDto> CreateQuestAsync(CreateQuestDto dto) =>
            _http.PostAsync<QuestDto>("api/quests", dto)!;

        public Task<QuestDto> UpdateQuestAsync(Guid questId, UpdateQuestDto dto) =>
            _http.PutAsync<QuestDto>($"api/quests/{questId}", dto)!;

        public Task DeleteQuestAsync(Guid questId) =>
            _http.DeleteAsync($"api/quests/{questId}");

        public Task SendToAllAsync(Guid questId) =>
            _http.PostAsync($"api/quests/{questId}/send-all", new { });
    }
}
