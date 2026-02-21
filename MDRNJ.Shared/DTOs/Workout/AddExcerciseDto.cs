using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MDRNJ.Shared.DTOs.Workout
{
    public class AddExerciseDto
    {
        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Category { get; set; }

        [Range(1, 100)]
        public int? Sets { get; set; }

        [Range(1, 10000)]
        public int? Reps { get; set; }

        [Range(0, 1000)]
        public float? WeightKg { get; set; }

        [Range(0, 1000)]
        public float? DistanceKm { get; set; }

        [Range(1, 86400)]
        public int? DurationSecs { get; set; }
    }
}
