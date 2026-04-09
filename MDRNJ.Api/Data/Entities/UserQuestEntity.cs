using MDRNJ.Shared.Enums;

namespace MDRNJ.Api.Data.Entities
{
    public class UserQuestEntity
    {
        public Guid UserId { get; set; }
        public Guid QuestId { get; set; }
        public float Progress { get; set; }
        public QuestStatus Status { get; set; } = QuestStatus.Active;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public UserEntity User { get; set; } = null!;
        public QuestEntity Quest { get; set; } = null!;
    }
}
