namespace MDRNJ.Shared.DTOs.Quest
{
    public class QuestDto
    {
        public Guid QuestId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int XPReward { get; set; }
        public string QuestType { get; set; } = null!;
        public float TargetValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AssignedCount { get; set; }
    }
}
