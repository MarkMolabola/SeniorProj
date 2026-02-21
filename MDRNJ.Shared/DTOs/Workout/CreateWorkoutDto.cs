using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MDRNJ.Shared.DTOs.Workout
{
    public class CreateWorkoutDto
    {
        [Required]
        [MaxLength(50)]
        public required string WorkoutType { get; set; }

        [Required]
        public required DateTime StartTime { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
