using MDRNJ.Shared.DTOs.Quest;

namespace MDRNJfitness.Services.Interfaces
{
    public interface IQuestService
    {
        // User
        Task<List<UserQuestDto>> GetMyQuestsAsync();

        // Admin
        Task<List<QuestDto>> GetAllQuestsAsync();
        Task<QuestDto> CreateQuestAsync(CreateQuestDto dto);
        Task<QuestDto> UpdateQuestAsync(Guid questId, UpdateQuestDto dto);
        Task DeleteQuestAsync(Guid questId);
        Task SendToAllAsync(Guid questId);
    }
}
