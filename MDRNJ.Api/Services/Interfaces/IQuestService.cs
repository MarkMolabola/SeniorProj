using MDRNJ.Shared.DTOs.Quest;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IQuestService
    {
        // Admin operations
        Task<QuestDto> CreateAsync(CreateQuestDto dto);
        Task<QuestDto> UpdateAsync(Guid questId, UpdateQuestDto dto);
        Task DeleteAsync(Guid questId);
        Task<List<QuestDto>> GetAllAsync();
        Task SendToAllUsersAsync(Guid questId);
        Task SendToUserAsync(Guid questId, Guid userId);

        // User operations
        Task<List<UserQuestDto>> GetMyQuestsAsync(Guid userId);
        Task<UserQuestDto> UpdateProgressAsync(Guid userId, Guid questId, float progress);
    }
}
