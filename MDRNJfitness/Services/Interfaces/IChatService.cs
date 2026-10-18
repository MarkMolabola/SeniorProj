using MDRNJ.Shared.DTOs.Chat;

namespace MDRNJfitness.Services.Interfaces
{
    public interface IChatService
    {
        Task<List<ChatChannelDto>> GetChannelsAsync();
        Task<List<ChatMessageDto>> GetMessagesAsync(Guid channelId, DateTime? before = null);
        Task<ChatMessageDto> SendMessageAsync(Guid channelId, string body);
        Task MarkReadAsync(Guid channelId);
        Task<List<ChatMemberDto>> GetMembersAsync(Guid channelId);
        Task<ChatChannelDto> OpenDirectAsync(Guid otherUserId);
        Task<ChatChannelDto> CreateGroupAsync(string name, List<Guid> memberIds);
        Task<List<ChatMemberDto>> AddMembersAsync(Guid channelId, List<Guid> userIds);
        Task LeaveGroupAsync(Guid channelId);

        // Moderation
        Task ReportMessageAsync(Guid messageId, string reason);
        Task DeleteMessageAsync(Guid messageId);
        Task<List<ReportedMessageDto>> GetReportsAsync();
        Task DismissReportsAsync(Guid messageId);
        Task MuteUserAsync(Guid userId, int hours);
        Task SetBannedAsync(Guid userId, bool banned);
    }
}
