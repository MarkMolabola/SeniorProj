using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Hubs;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Chat;
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
    /// Global, group and direct chats. Direct chats and group invites are limited to friends.
    /// Unknown chats and chats the user isn't in both read as "not found".
    /// </summary>
    public class ChatService : IChatService
    {
        private const string NotFound = "Chat not found.";

        private readonly AppDbContext _db;
        private readonly IHubContext<ChatHub> _hub;

        public ChatService(AppDbContext db, IHubContext<ChatHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        public async Task<List<ChatChannelDto>> GetChannelsAsync(Guid userId)
        {
            var global = await _db.ChatChannels.FirstAsync(c => c.ChannelId == ChatRules.GlobalChannelId);

            var mine = await _db.ChatMembers
                .Where(m => m.UserId == userId)
                .Select(m => new
                {
                    m.Channel,
                    MemberCount = m.Channel.Members.Count,
                    Other = m.Channel.Members
                        .Where(x => x.UserId != userId)
                        .Select(x => new { x.UserId, x.User.Username })
                        .FirstOrDefault(),
                    Unread = m.Channel.Messages.Count(x => x.SenderId != userId && x.SentAt > m.LastReadAt)
                })
                .ToListAsync();

            var rows = mine
                .Select(x => ToDto(x.Channel, userId, x.MemberCount, x.Other?.UserId, x.Other?.Username, x.Unread))
                .OrderByDescending(c => c.LastMessageAt ?? DateTime.MinValue)
                .ToList();

            rows.Insert(0, ToDto(global, userId, 0, null, null, 0));
            return rows;
        }

        public async Task<bool> CanAccessAsync(Guid userId, Guid channelId) =>
            channelId == ChatRules.GlobalChannelId ||
            await _db.ChatMembers.AnyAsync(m => m.ChannelId == channelId && m.UserId == userId);

        public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid userId, Guid channelId, DateTime? before, int take)
        {
            await EnsureAccessAsync(userId, channelId);
            take = Math.Clamp(take, 1, 100);

            var query = _db.ChatMessages.Where(m => m.ChannelId == channelId);
            if (before is not null)
                query = query.Where(m => m.SentAt < before);

            var page = await query
                .OrderByDescending(m => m.SentAt)
                .Take(take)
                .Select(m => new ChatMessageDto
                {
                    MessageId = m.MessageId,
                    ChannelId = m.ChannelId,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.Username,
                    Body = m.Body,
                    SentAt = m.SentAt
                })
                .ToListAsync();

            // Opening the latest page counts as reading the chat
            if (before is null)
                await MarkReadAsync(userId, channelId);

            page.Reverse(); // oldest first, ready to display
            return page;
        }

        public async Task<ChatMessageDto> SendMessageAsync(Guid userId, Guid channelId, SendChatMessageDto dto)
        {
            var body = (dto.Body ?? string.Empty).Trim();
            if (body.Length == 0)
                throw new ArgumentException("Type a message first.");
            if (body.Length > ChatRules.MaxMessageLength)
                throw new ArgumentException($"Messages can be up to {ChatRules.MaxMessageLength} characters.");

            await EnsureAccessAsync(userId, channelId);
            var channel = await _db.ChatChannels.FirstAsync(c => c.ChannelId == channelId);

            var now = DateTime.UtcNow;
            var cutoff = now - ChatRules.RateLimitWindow;
            if (await _db.ChatMessages.CountAsync(m => m.SenderId == userId && m.SentAt > cutoff) >= ChatRules.RateLimitCount)
                throw new InvalidOperationException("You're sending messages too fast.");

            var senderName = await _db.Users.Where(u => u.UserId == userId).Select(u => u.Username).FirstAsync();

            var message = new ChatMessageEntity { ChannelId = channelId, SenderId = userId, Body = body, SentAt = now };
            _db.ChatMessages.Add(message);

            channel.LastMessageAt = now;
            channel.LastMessagePreview = body.Length <= 120 ? body : body[..117] + "...";
            channel.LastSenderName = senderName;

            var membership = await _db.ChatMembers.FirstOrDefaultAsync(m => m.ChannelId == channelId && m.UserId == userId);
            if (membership is not null)
                membership.LastReadAt = now;

            await _db.SaveChangesAsync();

            var result = new ChatMessageDto
            {
                MessageId = message.MessageId,
                ChannelId = channelId,
                SenderId = userId,
                SenderName = senderName,
                Body = body,
                SentAt = now
            };

            await _hub.Clients.Group(ChatHub.ChannelGroup(channelId)).SendAsync("MessageReceived", result);
            if (channel.Kind != ChatChannelKind.Global)
                await NotifyMembersAsync(channelId);

            return result;
        }

        public async Task MarkReadAsync(Guid userId, Guid channelId)
        {
            await _db.ChatMembers
                .Where(m => m.ChannelId == channelId && m.UserId == userId)
                .ExecuteUpdateAsync(u => u.SetProperty(m => m.LastReadAt, DateTime.UtcNow));
        }

        public async Task<List<ChatMemberDto>> GetMembersAsync(Guid userId, Guid channelId)
        {
            await EnsureAccessAsync(userId, channelId);
            if (channelId == ChatRules.GlobalChannelId)
                return new List<ChatMemberDto>();

            var ownerId = await _db.ChatChannels.Where(c => c.ChannelId == channelId).Select(c => c.OwnerId).FirstAsync();
            return await _db.ChatMembers
                .Where(m => m.ChannelId == channelId)
                .OrderBy(m => m.JoinedAt)
                .Select(m => new ChatMemberDto
                {
                    UserId = m.UserId,
                    Username = m.User.Username,
                    IsOwner = m.UserId == ownerId
                })
                .ToListAsync();
        }

        public async Task<ChatChannelDto> OpenDirectAsync(Guid userId, Guid otherUserId)
        {
            if (userId == otherUserId)
                throw new ArgumentException("You can't message yourself.");

            var other = await _db.Users.Where(u => u.UserId == otherUserId).Select(u => new { u.UserId, u.Username }).FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("User not found.");

            if (!await AreFriendsAsync(userId, otherUserId))
                throw new InvalidOperationException("You can only message friends. Send a friend request first.");

            var key = DirectKey(userId, otherUserId);
            var channel = await _db.ChatChannels.FirstOrDefaultAsync(c => c.DirectKey == key);
            if (channel is null)
            {
                channel = new ChatChannelEntity { Kind = ChatChannelKind.Direct, DirectKey = key };
                channel.Members.Add(new ChatMemberEntity { UserId = userId });
                channel.Members.Add(new ChatMemberEntity { UserId = otherUserId });
                _db.ChatChannels.Add(channel);
                await _db.SaveChangesAsync();
            }

            return ToDto(channel, userId, 2, other.UserId, other.Username, 0);
        }

        public async Task<ChatChannelDto> CreateGroupAsync(Guid userId, CreateChatGroupDto dto)
        {
            var name = (dto.Name ?? string.Empty).Trim();
            if (name.Length == 0)
                throw new ArgumentException("Give your group a name.");
            if (name.Length > ChatRules.MaxGroupNameLength)
                throw new ArgumentException($"Group names can be up to {ChatRules.MaxGroupNameLength} characters.");

            var memberIds = dto.MemberIds.Where(id => id != userId).Distinct().ToList();
            if (memberIds.Count + 1 > ChatRules.MaxGroupMembers)
                throw new ArgumentException($"Groups can have up to {ChatRules.MaxGroupMembers} members.");
            await EnsureAllFriendsAsync(userId, memberIds);

            var channel = new ChatChannelEntity { Kind = ChatChannelKind.Group, Name = name, OwnerId = userId };
            channel.Members.Add(new ChatMemberEntity { UserId = userId });
            foreach (var id in memberIds)
                channel.Members.Add(new ChatMemberEntity { UserId = id });

            _db.ChatChannels.Add(channel);
            await _db.SaveChangesAsync();

            await NotifyMembersAsync(channel.ChannelId);
            return ToDto(channel, userId, channel.Members.Count, null, null, 0);
        }

        public async Task<List<ChatMemberDto>> AddMembersAsync(Guid userId, Guid channelId, AddChatMembersDto dto)
        {
            var channel = await GetGroupForMemberAsync(userId, channelId);

            var existing = await _db.ChatMembers.Where(m => m.ChannelId == channelId).Select(m => m.UserId).ToListAsync();
            var newIds = dto.UserIds.Distinct().Except(existing).ToList();
            if (newIds.Count == 0)
                throw new ArgumentException("They're already in this group.");
            if (existing.Count + newIds.Count > ChatRules.MaxGroupMembers)
                throw new ArgumentException($"Groups can have up to {ChatRules.MaxGroupMembers} members.");
            await EnsureAllFriendsAsync(userId, newIds);

            foreach (var id in newIds)
                _db.ChatMembers.Add(new ChatMemberEntity { ChannelId = channel.ChannelId, UserId = id });
            await _db.SaveChangesAsync();

            await NotifyMembersAsync(channelId);
            return await GetMembersAsync(userId, channelId);
        }

        public async Task LeaveGroupAsync(Guid userId, Guid channelId)
        {
            var channel = await GetGroupForMemberAsync(userId, channelId);

            var members = await _db.ChatMembers.Where(m => m.ChannelId == channelId).OrderBy(m => m.JoinedAt).ToListAsync();
            var leaving = members.First(m => m.UserId == userId);
            _db.ChatMembers.Remove(leaving);

            var remaining = members.Where(m => m.UserId != userId).ToList();
            if (remaining.Count == 0)
                _db.ChatChannels.Remove(channel); // last one out: the group and its messages go
            else if (channel.OwnerId == userId)
                channel.OwnerId = remaining[0].UserId;

            await _db.SaveChangesAsync();
            if (remaining.Count > 0)
                await NotifyMembersAsync(channelId);
        }

        // ── Helpers ─────────────────────────────────────────────

        private async Task EnsureAccessAsync(Guid userId, Guid channelId)
        {
            if (!await CanAccessAsync(userId, channelId))
                throw new KeyNotFoundException(NotFound);
        }

        private async Task<ChatChannelEntity> GetGroupForMemberAsync(Guid userId, Guid channelId)
        {
            await EnsureAccessAsync(userId, channelId);
            var channel = await _db.ChatChannels.FirstAsync(c => c.ChannelId == channelId);
            if (channel.Kind != ChatChannelKind.Group)
                throw new ArgumentException("Only group chats have members to manage.");
            return channel;
        }

        private Task<bool> AreFriendsAsync(Guid userId, Guid otherUserId) =>
            _db.Friendships.AnyAsync(f =>
                ((f.RequesterId == userId && f.AddresseeId == otherUserId) ||
                 (f.RequesterId == otherUserId && f.AddresseeId == userId)) &&
                f.Status == FriendshipStatus.Accepted);

        private async Task EnsureAllFriendsAsync(Guid userId, List<Guid> ids)
        {
            if (ids.Count == 0) return;

            var friendIds = await _db.Friendships
                .Where(f => (f.RequesterId == userId || f.AddresseeId == userId) && f.Status == FriendshipStatus.Accepted)
                .Select(f => f.RequesterId == userId ? f.AddresseeId : f.RequesterId)
                .ToListAsync();

            if (ids.Except(friendIds).Any())
                throw new InvalidOperationException("You can only add friends to a group.");
        }

        /// <summary>Tells every member's devices that this chat changed, so their chat lists refresh.</summary>
        private async Task NotifyMembersAsync(Guid channelId)
        {
            var memberIds = await _db.ChatMembers
                .Where(m => m.ChannelId == channelId)
                .Select(m => m.UserId.ToString())
                .ToListAsync();
            await _hub.Clients.Users(memberIds).SendAsync("ChannelActivity", channelId);
        }

        private static string DirectKey(Guid a, Guid b)
        {
            var (first, second) = string.CompareOrdinal(a.ToString(), b.ToString()) < 0 ? (a, b) : (b, a);
            return $"{first}_{second}";
        }

        private static ChatChannelDto ToDto(ChatChannelEntity c, Guid userId, int memberCount,
            Guid? otherUserId, string? otherUsername, int unread) => new()
        {
            ChannelId = c.ChannelId,
            Kind = c.Kind.ToString(),
            Name = c.Kind == ChatChannelKind.Direct ? otherUsername ?? "Deleted user" : c.Name ?? "Chat",
            OtherUserId = otherUserId,
            MemberCount = memberCount,
            IsOwner = c.OwnerId == userId,
            LastMessagePreview = c.LastMessagePreview,
            LastSenderName = c.LastSenderName,
            LastMessageAt = c.LastMessageAt,
            UnreadCount = unread
        };
    }
}
