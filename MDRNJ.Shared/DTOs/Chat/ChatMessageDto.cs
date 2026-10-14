namespace MDRNJ.Shared.DTOs.Chat
{
    public class ChatMessageDto
    {
        public Guid MessageId { get; set; }
        public Guid ChannelId { get; set; }
        public Guid SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
