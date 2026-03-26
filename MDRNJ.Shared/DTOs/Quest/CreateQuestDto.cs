using System.ComponentModel.DataAnnotations;

namespace MDRNJ.Shared.DTOs.Quest
{
    public class CreateQuestDto
    {
        [Required]
        [MaxLength(100)]
        public required string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int XPReward { get; set; }

        [Required]
        public required string QuestType { get; set; }

        [Range(0, float.MaxValue)]
        public float TargetValue { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
