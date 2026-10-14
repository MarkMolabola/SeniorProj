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
        public int Gold { get; set; } = MDRNJ.Shared.Game.GoldRules.StartingGold;
        public DateTime? LastWorkoutDate { get; set; }

        /// <summary>When dungeon gold was last collected; gold builds up from here (DungeonRules).</summary>
        public DateTime DungeonCollectedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public UserEntity User { get; set; } = null!;
    }
}
