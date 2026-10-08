using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.DTOs.Character
{
    public class ActivityScoreDto
    {
        public Guid UserId { get; set; }
        public float StrengthScore { get; set; }
        public float CardioScore { get; set; }
        public float FlexibilityScore { get; set; }

        // Stat points: only go up (see MDRNJ.Shared.Game.StatProgression)
        public float StrengthPoints { get; set; }
        public float CardioPoints { get; set; }
        public float FlexibilityPoints { get; set; }

        public string DominantSport { get; set; }
        public DateTime LastCalculated { get; set; }
    }
}
