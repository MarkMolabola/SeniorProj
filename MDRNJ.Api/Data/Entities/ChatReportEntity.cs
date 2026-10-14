using System;

namespace MDRNJ.Api.Data.Entities
{
    /// <summary>A player's report of a chat message (one per player per message).</summary>
    public class ChatReportEntity
    {
        public Guid ReportId { get; set; } = Guid.NewGuid();
        public Guid MessageId { get; set; }
        public Guid ReporterId { get; set; }
        public string Reason { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ChatMessageEntity Message { get; set; } = null!;
        public UserEntity Reporter { get; set; } = null!;
    }
}
