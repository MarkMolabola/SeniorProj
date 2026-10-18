namespace MDRNJ.Shared.DTOs.Chat
{
    /// <summary>A reported chat message waiting for an admin (Chat ▸ REPORTS).</summary>
    public class ReportedMessageDto
    {
        public Guid MessageId { get; set; }
        public Guid ChannelId { get; set; }
        public string ChannelName { get; set; } = string.Empty;
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }

        /// <summary>Auto-hidden after ChatRules.ReportsToHide reports.</summary>
        public bool IsHidden { get; set; }

        /// <summary>The sender's current chat mute, if any.</summary>
        public DateTime? SenderMutedUntil { get; set; }
        public bool SenderBanned { get; set; }
        public int ReportCount { get; set; }
        public List<string> Reasons { get; set; } = new();
    }
}
