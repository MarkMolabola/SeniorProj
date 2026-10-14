using System;

namespace MDRNJ.Api.Data.Entities
{
    public class ChatMessageEntity
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public Guid ChannelId { get; set; }
        public Guid SenderId { get; set; }
        public string Body { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ChatChannelEntity Channel { get; set; } = null!;
        public UserEntity Sender { get; set; } = null!;
    }
}
