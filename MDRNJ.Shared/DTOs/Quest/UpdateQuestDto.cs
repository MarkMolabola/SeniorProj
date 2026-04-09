using System.ComponentModel.DataAnnotations;

namespace MDRNJ.Shared.DTOs.Quest
{
    public class UpdateQuestDto
    {
        [MaxLength(100)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? XPReward { get; set; }
        public string? QuestType { get; set; }
        public float? TargetValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsActive { get; set; }
    }
}
