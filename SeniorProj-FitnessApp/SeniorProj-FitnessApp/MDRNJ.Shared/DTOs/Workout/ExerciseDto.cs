using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.DTOs.Workout
{
    public class ExerciseDto
    {
        public Guid ExerciseId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public int? Sets { get; set; }
        public int? Reps { get; set; }
        public float? WeightKg { get; set; }
        public float? DistanceKm { get; set; }
        public int? DurationSecs { get; set; }
    }
}
