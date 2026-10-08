namespace MDRNJ.Shared.Game
{
    /// <summary>
    /// Account (hero) level from total XP: reaching level N+1 takes N² × 100 XP.
    /// Shared so the API and the app agree on the maths.
    /// </summary>
    public static class AccountProgression
    {
        public static int LevelFor(int totalXP)
        {
            int level = 1;
            while (totalXP >= XPForNextLevel(level)) level++;
            return level;
        }

        /// <summary>Total XP needed to leave <paramref name="level"/>.</summary>
        public static int XPForNextLevel(int level) => level * level * 100;
    }
}
