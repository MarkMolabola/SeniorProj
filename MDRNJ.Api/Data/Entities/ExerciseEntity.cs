using System;

namespace MDRNJ.Api.Data.Entities
{
    public class ExerciseEntity
    {
        public Guid ExerciseId { get; set; } = Guid.NewGuid();
        public Guid SessionId { get; set; }
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
        public int? Sets { get; set; }
        public int? Reps { get; set; }
        public float? WeightKg { get; set; }
        public float? DistanceKm { get; set; }
        public int? DurationSecs { get; set; }

        // Navigation
        public WorkoutSessionEntity Session { get; set; } = null!;
    }
}
