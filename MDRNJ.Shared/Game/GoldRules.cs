namespace MDRNJ.Shared.Game
{
    /// <summary>How players earn gold, the currency spent in the shop.</summary>
    public static class GoldRules
    {
        /// <summary>Every new account (and every existing one, via migration) starts with this.</summary>
        public const int StartingGold = 100;

        /// <summary>Reaching a level pays level × 50 (level 2 = 100, level 10 = 500).</summary>
        public static int ForLevelUp(int newLevel) => newLevel * 50;

        /// <summary>1 gold per workout minute, at least 5.</summary>
        public static int ForWorkout(int durationMins) => Math.Max(5, durationMins);

        /// <summary>Half the quest's XP reward.</summary>
        public static int ForQuest(int xpReward) => xpReward / 2;

        /// <summary>Half the achievement's XP reward.</summary>
        public static int ForAchievement(int xpReward) => xpReward / 2;

        /// <summary>Win 50 / tie 25 / loss 10; nothing if you never submitted.</summary>
        public static int ForDuel(string? outcome, bool competed) =>
            !competed ? 0 : outcome switch
            {
                "Won" => 50,
                "Tie" => 25,
                _ => 10
            };
    }
}
