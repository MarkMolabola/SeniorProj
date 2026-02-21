using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MDRNJ.Shared.DTOs.Challenge
{
    public class CreateChallengeDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(100)]
        public required string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public required string ChallengeType { get; set; }

        [Required]
        [Range(0.01, 100000)]
        public required float TargetValue { get; set; }

        [Range(0, 10000)]
        public int XPReward { get; set; }

        [Required]
        public required DateTime StartDate { get; set; }

        [Required]
        public required DateTime EndDate { get; set; }

        public List<Guid>? InvitedUserIds { get; set; }
    }
}
