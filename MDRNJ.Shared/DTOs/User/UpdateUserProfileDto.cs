using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MDRNJ.Shared.DTOs.User
{
    public class UpdateUserProfileDto
    {
        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public required string DisplayName { get; set; }

        [MaxLength(300)]
        public string? Bio { get; set; }

        [Range(20, 500)]
        public float? WeightKg { get; set; }

        [Range(50, 300)]
        public float? HeightCm { get; set; }

        [Required]
        [MaxLength(50)]
        public required string FitnessGoal { get; set; }
    }
}
