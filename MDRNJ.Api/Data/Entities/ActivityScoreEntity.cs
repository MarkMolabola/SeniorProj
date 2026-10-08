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

        // Stat points (STR / END / FLX) — only go up. The *Score fields above are
        // recent training shares (sum to 100) used to pick body type and outfit.
        public float StrengthPoints { get; set; }
        public float CardioPoints { get; set; }
        public float FlexibilityPoints { get; set; }
        public SportType DominantSport { get; set; } = SportType.Cardio;
        public DateTime LastCalculated { get; set; } = DateTime.UtcNow;

        // Navigation
        public UserEntity User { get; set; } = null!;
    }
}
