using System;

namespace MDRNJ.Api.Data.Entities
{
    /// <summary>A user's membership in a group or direct chat.</summary>
    public class ChatMemberEntity
    {
        public Guid ChannelId { get; set; }
        public Guid UserId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Messages after this (from others) count as unread.</summary>
        public DateTime LastReadAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ChatChannelEntity Channel { get; set; } = null!;
        public UserEntity User { get; set; } = null!;
    }
}
