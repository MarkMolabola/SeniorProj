using MDRNJ.Shared.DTOs.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IChatService
    {
        Task<List<ChatChannelDto>> GetChannelsAsync(Guid userId);
        Task<bool> CanAccessAsync(Guid userId, Guid channelId);
        Task<List<ChatMessageDto>> GetMessagesAsync(Guid userId, Guid channelId, DateTime? before, int take);
        Task<ChatMessageDto> SendMessageAsync(Guid userId, Guid channelId, SendChatMessageDto dto);
        Task MarkReadAsync(Guid userId, Guid channelId);
        Task<List<ChatMemberDto>> GetMembersAsync(Guid userId, Guid channelId);
        Task<ChatChannelDto> OpenDirectAsync(Guid userId, Guid otherUserId);
        Task<ChatChannelDto> CreateGroupAsync(Guid userId, CreateChatGroupDto dto);
        Task<List<ChatMemberDto>> AddMembersAsync(Guid userId, Guid channelId, AddChatMembersDto dto);
        Task LeaveGroupAsync(Guid userId, Guid channelId);
    }
}
