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

        /// <summary>Free stat points from levelling up (AccountProgression.StatPointsFor), not yet spent.</summary>
        public int UnspentStatPoints { get; set; }

        /// <summary>When dungeon gold was last collected; gold builds up from here (DungeonRules).</summary>
        public DateTime DungeonCollectedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Current dungeon floor (DungeonRules.Floors) and kills made on it so far.</summary>
        public int DungeonFloor { get; set; } = 1;
        public int DungeonFloorKills { get; set; }

        /// <summary>The weekly boss being fought: its week (Monday UTC), damage dealt, and whether it fell.</summary>
        public DateTime? DungeonBossWeek { get; set; }
        public int DungeonBossDamage { get; set; }
        public bool DungeonBossDefeated { get; set; }

        // Navigation
        public UserEntity User { get; set; } = null!;
    }
}
