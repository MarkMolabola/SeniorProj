using System;

namespace MDRNJ.Api.Data.Entities
{
    public class GameStatsEntity
    {
        public Guid StatsId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public int TotalXP { get; set; }
        public int CurrentLevel { get; set; } = 1;
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TotalWorkouts { get; set; }
        public DateTime? LastWorkoutDate { get; set; }

        // Navigation
        public UserEntity User { get; set; } = null!;
    }
}
