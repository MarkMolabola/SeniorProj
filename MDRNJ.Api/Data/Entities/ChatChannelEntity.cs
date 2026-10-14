using MDRNJ.Shared.Enums;
using System;
using System.Collections.Generic;

namespace MDRNJ.Api.Data.Entities
{
    /// <summary>
    /// A chat room: the one global room, a group (guild) chat, or a direct chat between two friends.
    /// Global has no member rows; everyone can use it.
    /// </summary>
    public class ChatChannelEntity
    {
        public Guid ChannelId { get; set; } = Guid.NewGuid();
        public ChatChannelKind Kind { get; set; }

        /// <summary>Group name; null for global and direct chats.</summary>
        public string? Name { get; set; }

        /// <summary>Group owner; passes to the longest-standing member when the owner leaves.</summary>
        public Guid? OwnerId { get; set; }

        /// <summary>"{smallerId}_{largerId}" for direct chats, so each pair of friends shares one chat.</summary>
        public string? DirectKey { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Latest message, kept here so the chat list doesn't scan messages
        public DateTime? LastMessageAt { get; set; }
        public string? LastMessagePreview { get; set; }
        public string? LastSenderName { get; set; }

        // Navigation
        public ICollection<ChatMemberEntity> Members { get; set; } = new List<ChatMemberEntity>();
        public ICollection<ChatMessageEntity> Messages { get; set; } = new List<ChatMessageEntity>();
    }
}
