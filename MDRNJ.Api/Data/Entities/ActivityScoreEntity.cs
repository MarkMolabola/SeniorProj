using MDRNJ.Shared.Enums;
using System;

namespace MDRNJ.Api.Data.Entities
{
    public class ActivityScoreEntity
    {
        public Guid ScoreId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public float StrengthScore { get; set; }
        public float CardioScore { get; set; }
        public float FlexibilityScore { get; set; }
        public SportType DominantSport { get; set; } = SportType.Cardio;
        public DateTime LastCalculated { get; set; } = DateTime.UtcNow;

        // Navigation
        public UserEntity User { get; set; } = null!;
    }
}
