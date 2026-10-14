namespace MDRNJ.Shared.DTOs.Chat
{
    /// <summary>A row in the chat list: the global room, a group, or a direct chat.</summary>
    public class ChatChannelDto
    {
        public Guid ChannelId { get; set; }

        /// <summary>ChatChannelKind name: Global, Group or Direct.</summary>
        public string Kind { get; set; } = "Global";

        /// <summary>Group name, or the other person's username for a direct chat.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>The other person in a direct chat.</summary>
        public Guid? OtherUserId { get; set; }

        public int MemberCount { get; set; }
        public bool IsOwner { get; set; }

        public string? LastMessagePreview { get; set; }
        public string? LastSenderName { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
    }
}
