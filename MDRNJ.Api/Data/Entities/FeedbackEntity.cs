using MDRNJ.Shared.Enums;
using System;

namespace MDRNJ.Api.Data.Entities
{
    /// <summary>A bug report or suggestion sent from Profile ▸ SEND FEEDBACK.</summary>
    public class FeedbackEntity
    {
        public Guid FeedbackId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public FeedbackKind Kind { get; set; }
        public string Message { get; set; } = null!;
        public string? AppVersion { get; set; }
        public string? Platform { get; set; }
        public string? DeviceModel { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; }

        // Navigation
        public UserEntity User { get; set; } = null!;
    }
}
