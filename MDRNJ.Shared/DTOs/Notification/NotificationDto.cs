namespace MDRNJ.Shared.DTOs.Notification
{
    /// <summary>A live in-app notification, pushed over the notifications hub.</summary>
    public class NotificationDto
    {
        /// <summary>A <see cref="MDRNJ.Shared.Enums.NotificationType"/> name.</summary>
        public string Type { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        /// <summary>The player who caused it (sender, challenger, ...).</summary>
        public Guid? FromUserId { get; set; }

        /// <summary>The challenge or friendship it's about.</summary>
        public Guid? RelatedId { get; set; }
    }
}
