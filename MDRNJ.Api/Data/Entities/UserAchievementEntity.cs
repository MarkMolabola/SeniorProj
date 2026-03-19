using System;

namespace MDRNJ.Api.Data.Entities
{
    public class UserAchievementEntity
    {
        public Guid UserId { get; set; }
        public Guid AchievementId { get; set; }
        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public UserEntity User { get; set; } = null!;
        public AchievementEntity Achievement { get; set; } = null!;
    }
}
