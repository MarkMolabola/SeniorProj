using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.DTOs.Game
{
    public class GameStatsDto
    {
        public Guid UserId { get; set; }
        public int TotalXP { get; set; }
        public int CurrentLevel { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TotalWorkouts { get; set; }
        public DateTime? LastWorkoutDate { get; set; }
        public int XPToNextLevel { get; set; }
        public int Gold { get; set; }

        /// <summary>Free stat points from levelling up, not yet put into a stat.</summary>
        public int UnspentStatPoints { get; set; }

        /// <summary>Admins shop for free; the app shows their gold as ∞.</summary>
        public bool UnlimitedGold { get; set; }
    }
}
