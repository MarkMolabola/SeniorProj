namespace MDRNJ.Shared.DTOs.Game
{
    /// <summary>
    /// What changed on the account after a workout, quest or challenge: XP
    /// earned, account level before / after, and any goals it completed.
    /// Returned alongside those results so the app can show the reward popup.
    /// </summary>
    public class AccountProgressDto
    {
        /// <summary>All XP earned, including quest / challenge rewards.</summary>
        public int XPGained { get; set; }
        public int GoldGained { get; set; }
        public int PreviousLevel { get; set; }
        public int NewLevel { get; set; }
        public bool LeveledUp => NewLevel > PreviousLevel;

        public List<CompletedGoalDto> CompletedGoals { get; set; } = new();
    }

    public class CompletedGoalDto
    {
        /// <summary>"Quest" or "Challenge".</summary>
        public string Kind { get; set; } = null!;
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public int XPReward { get; set; }
    }
}
