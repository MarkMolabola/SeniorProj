using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.DTOs.Workout
{
    public class WorkoutDto
    {
        public Guid SessionId { get; set; }
        public Guid UserId { get; set; }
        public string WorkoutType { get; set; }
        public int DurationMins { get; set; }
        public float? CaloriesBurned { get; set; }
        public int XPAwarded { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Notes { get; set; }
        public List<ExerciseDto> Exercises { get; set; } = new();
    }
}
