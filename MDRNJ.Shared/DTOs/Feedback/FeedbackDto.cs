using MDRNJ.Shared.Enums;

namespace MDRNJ.Shared.DTOs.Feedback
{
    /// <summary>One piece of feedback as admins see it (Profile ▸ FEEDBACK INBOX).</summary>
    public class FeedbackDto
    {
        public Guid FeedbackId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public FeedbackKind Kind { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AppVersion { get; set; }
        public string? Platform { get; set; }
        public string? DeviceModel { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }
}
