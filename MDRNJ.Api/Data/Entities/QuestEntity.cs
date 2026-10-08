using MDRNJ.Shared.Enums;

namespace MDRNJ.Api.Data.Entities
{
    public class QuestEntity
    {
        public Guid QuestId { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int XPReward { get; set; }
        public ChallengeType QuestType { get; set; }
        public float TargetValue { get; set; }

        /// <summary>Only workouts of this type count; null means any workout.</summary>
        public WorkoutType? WorkoutType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<UserQuestEntity> UserQuests { get; set; } = new List<UserQuestEntity>();
    }
}
