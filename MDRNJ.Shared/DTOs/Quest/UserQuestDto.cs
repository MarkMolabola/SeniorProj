namespace MDRNJ.Shared.DTOs.Quest
{
    public class UserQuestDto
    {
        public Guid QuestId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int XPReward { get; set; }
        public string QuestType { get; set; } = null!;
        public float TargetValue { get; set; }
        public float Progress { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        /// <summary>Set when this update completed the quest.</summary>
        public MDRNJ.Shared.DTOs.Character.CharacterProgressDto? CharacterProgress { get; set; }

        /// <summary>Set when this update completed the quest.</summary>
        public MDRNJ.Shared.DTOs.Game.AccountProgressDto? AccountProgress { get; set; }
    }
}
