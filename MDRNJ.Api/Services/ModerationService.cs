using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Hubs;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Chat;
using MDRNJ.Shared.DTOs.Friend;
using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    /// <summary>
    /// Chat safety: reporting (auto-hide after ChatRules.ReportsToHide reports), deleting
    /// messages (own, or any for admins), the admin report queue, and blocking players.
    /// </summary>
    public class ModerationService : IModerationService
    {
        private readonly AppDbContext _db;
        private readonly IChatService _chat;
        private readonly IHubContext<ChatHub> _hub;

        public ModerationService(AppDbContext db, IChatService chat, IHubContext<ChatHub> hub)
        {
            _db = db;
            _chat = chat;
            _hub = hub;
        }

        public async Task ReportMessageAsync(Guid userId, Guid messageId, ReportChatMessageDto dto)
        {
            if (!ChatRules.ReportReasons.Contains(dto.Reason))
                throw new ArgumentException("Pick a reason for the report.");

            var message = await FindVisibleMessageAsync(userId, messageId);
            if (message.SenderId == userId)
                throw new ArgumentException("You can't report your own message.");
            if (await _db.ChatReports.AnyAsync(r => r.MessageId == messageId && r.ReporterId == userId))
                throw new InvalidOperationException("You already reported this message.");

            _db.ChatReports.Add(new ChatReportEntity { MessageId = messageId, ReporterId = userId, Reason = dto.Reason });
            await _db.SaveChangesAsync();

            var reports = await _db.ChatReports.CountAsync(r => r.MessageId == messageId);
            if (!message.IsHidden && reports >= ChatRules.ReportsToHide)
            {
                message.IsHidden = true;
                await _db.SaveChangesAsync(); // before the preview query reads it
                await _chat.RefreshPreviewAsync(message.ChannelId);
                await NotifyRemovedAsync(message);
            }
        }

        public async Task DeleteMessageAsync(Guid userId, Guid messageId)
        {
            var message = await _db.ChatMessages.FirstOrDefaultAsync(m => m.MessageId == messageId)
                ?? throw new KeyNotFoundException("Message not found.");

            var isAdmin = await _db.Users.Where(u => u.UserId == userId).Select(u => u.IsAdmin).FirstOrDefaultAsync();
            if (message.SenderId != userId && !isAdmin)
                throw new KeyNotFoundException("Message not found.");

            _db.ChatMessages.Remove(message); // its reports go with it
            await _db.SaveChangesAsync();
            await _chat.RefreshPreviewAsync(message.ChannelId);
            await NotifyRemovedAsync(message);
        }

        public async Task<List<ReportedMessageDto>> GetReportsAsync()
        {
            var rows = await _db.ChatReports
                .GroupBy(r => r.MessageId)
                .Select(g => new
                {
                    MessageId = g.Key,
                    Count = g.Count(),
                    Reasons = g.Select(r => r.Reason).Distinct().ToList(),
                    Latest = g.Max(r => r.CreatedAt)
                })
                .ToListAsync();

            var ids = rows.Select(r => r.MessageId).ToList();
            var messages = await _db.ChatMessages
                .Where(m => ids.Contains(m.MessageId))
                .Select(m => new
                {
                    m.MessageId, m.ChannelId, m.SenderId, SenderName = m.Sender.Username,
                    m.Body, m.SentAt, m.IsHidden, m.Channel.Kind, m.Channel.Name
                })
                .ToDictionaryAsync(m => m.MessageId);

            return rows
                .Where(r => messages.ContainsKey(r.MessageId))
                .OrderByDescending(r => r.Count).ThenByDescending(r => r.Latest)
                .Select(r =>
                {
                    var m = messages[r.MessageId];
                    return new ReportedMessageDto
                    {
                        MessageId = m.MessageId,
                        ChannelId = m.ChannelId,
                        ChannelName = m.Kind == ChatChannelKind.Direct ? "Direct message" : m.Name ?? "Chat",
                        SenderId = m.SenderId,
                        SenderName = m.SenderName,
                        Body = m.Body,
                        SentAt = m.SentAt,
                        IsHidden = m.IsHidden,
                        ReportCount = r.Count,
                        Reasons = r.Reasons
                    };
                })
                .ToList();
        }

        /// <summary>Admin decided the message is fine: clear its reports and show it again.</summary>
        public async Task DismissReportsAsync(Guid messageId)
        {
            var message = await _db.ChatMessages.FirstOrDefaultAsync(m => m.MessageId == messageId)
                ?? throw new KeyNotFoundException("Message not found.");

            await _db.ChatReports.Where(r => r.MessageId == messageId).ExecuteDeleteAsync();
            if (message.IsHidden)
            {
                message.IsHidden = false;
                await _db.SaveChangesAsync(); // before the preview query reads it
                await _chat.RefreshPreviewAsync(message.ChannelId);
            }
        }

        public async Task BlockAsync(Guid userId, Guid blockedUserId)
        {
            if (userId == blockedUserId)
                throw new ArgumentException("You can't block yourself.");
            if (!await _db.Users.AnyAsync(u => u.UserId == blockedUserId))
                throw new KeyNotFoundException("User not found.");

            if (!await _db.UserBlocks.AnyAsync(b => b.BlockerId == userId && b.BlockedId == blockedUserId))
                _db.UserBlocks.Add(new UserBlockEntity { BlockerId = userId, BlockedId = blockedUserId });

            // Blocking ends any friendship or pending request, which also stops DMs and group invites
            await _db.Friendships
                .Where(f => (f.RequesterId == userId && f.AddresseeId == blockedUserId) ||
                            (f.RequesterId == blockedUserId && f.AddresseeId == userId))
                .ExecuteDeleteAsync();
            await _db.SaveChangesAsync();
        }

        public async Task UnblockAsync(Guid userId, Guid blockedUserId) =>
            await _db.UserBlocks
                .Where(b => b.BlockerId == userId && b.BlockedId == blockedUserId)
                .ExecuteDeleteAsync();

        public Task<List<BlockedUserDto>> GetBlockedAsync(Guid userId) =>
            _db.UserBlocks
                .Where(b => b.BlockerId == userId)
                .OrderBy(b => b.Blocked.Username)
                .Select(b => new BlockedUserDto { UserId = b.BlockedId, Username = b.Blocked.Username })
                .ToListAsync();

        // ── Helpers ─────────────────────────────────────────────

        private async Task<ChatMessageEntity> FindVisibleMessageAsync(Guid userId, Guid messageId)
        {
            var message = await _db.ChatMessages.FirstOrDefaultAsync(m => m.MessageId == messageId);
            if (message is null || !await _chat.CanAccessAsync(userId, message.ChannelId))
                throw new KeyNotFoundException("Message not found.");
            return message;
        }

        /// <summary>Removes the message from open chats right away.</summary>
        private Task NotifyRemovedAsync(ChatMessageEntity message) =>
            _hub.Clients.Group(ChatHub.ChannelGroup(message.ChannelId))
                .SendAsync("MessageDeleted", message.ChannelId, message.MessageId);
    }
}
