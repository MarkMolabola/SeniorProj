namespace MDRNJfitness.Models.Social
{
    public class Challenge
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GoalType { get; set; } = string.Empty;

        public int TargetValue { get; set; }
        public int CurrentProgress { get; set; }

        public bool IsCompleted => CurrentProgress >= TargetValue;

        public int XpReward { get; set; }

        public DateTime WeekStartDate { get; set; }
        public string Difficulty { get; set; } = string.Empty;
    }
}
