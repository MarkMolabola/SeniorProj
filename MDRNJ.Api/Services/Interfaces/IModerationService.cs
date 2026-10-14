using MDRNJ.Shared.DTOs.Chat;
using MDRNJ.Shared.DTOs.Friend;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IModerationService
    {
        Task ReportMessageAsync(Guid userId, Guid messageId, ReportChatMessageDto dto);
        Task DeleteMessageAsync(Guid userId, Guid messageId);

        Task<List<ReportedMessageDto>> GetReportsAsync();
        Task DismissReportsAsync(Guid messageId);

        Task BlockAsync(Guid userId, Guid blockedUserId);
        Task UnblockAsync(Guid userId, Guid blockedUserId);
        Task<List<BlockedUserDto>> GetBlockedAsync(Guid userId);
    }
}
