using MDRNJ.Shared.Enums;
using System;
using System.Collections.Generic;

namespace MDRNJ.Api.Data.Entities
{
    public class WorkoutSessionEntity
    {
        public Guid SessionId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public WorkoutType WorkoutType { get; set; }
        public int DurationMins { get; set; }
        public float? CaloriesBurned { get; set; }
        public int XPAwarded { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public UserEntity User { get; set; } = null!;
        public ICollection<ExerciseEntity> Exercises { get; set; } = new List<ExerciseEntity>();
    }
}
