namespace MDRNJ.Shared.Game
{
    /// <summary>
    /// Account (hero) level from total XP: reaching level N+1 takes N² × 50 XP in total
    /// (level 10 ≈ 1 month, level 25 ≈ 8 months of regular training).
    /// Shared so the API and the app agree on the maths.
    /// Keep the curve and rewards in sync with the SQL in the LevelRewards migration.
    /// </summary>
    public static class AccountProgression
    {
        public const int XPPerLevelSquared = 50;

        /// <summary>Free stat points for reaching a level: 5 plus the level (level 10 = 15).</summary>
        public const int BaseStatPointsPerLevel = 5;

        /// <summary>Extra dungeon gold and kills per level above 1 (level 10 = +18%).</summary>
        public const float DungeonBoostPerLevel = 0.02f;

        public static int LevelFor(int totalXP)
        {
            int level = 1;
            while (totalXP >= XPForNextLevel(level)) level++;
            return level;
        }

        /// <summary>Total XP needed to leave <paramref name="level"/>.</summary>
        public static int XPForNextLevel(int level) => level * level * XPPerLevelSquared;

        /// <summary>Free stat points (spent on STR / END / FLX in the Hero tab) for reaching <paramref name="newLevel"/>.</summary>
        public static int StatPointsFor(int newLevel) => BaseStatPointsPerLevel + newLevel;

        /// <summary>Stat points for every level from <paramref name="fromLevel"/> + 1 up to <paramref name="toLevel"/>.</summary>
        public static int StatPointsBetween(int fromLevel, int toLevel)
        {
            var total = 0;
            for (var level = fromLevel + 1; level <= toLevel; level++)
                total += StatPointsFor(level);
            return total;
        }

        /// <summary>Dungeon gold / kills multiplier for an account level.</summary>
        public static float DungeonBoost(int level) => 1 + DungeonBoostPerLevel * (Math.Max(1, level) - 1);
    }
}
