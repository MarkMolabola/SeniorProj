using System;
using System.ComponentModel.DataAnnotations;

namespace MDRNJ.Shared.DTOs.Workout
{
    public class BacklogWorkoutDto
    {
        [Required]
        public required string WorkoutType { get; set; }

        [Required]
        public required DateTime StartTime { get; set; }

        [Range(1, 1440)]
        public int DurationMins { get; set; } = 30;

        /// <summary>Distance covered, for types where <see cref="MDRNJ.Shared.Game.WorkoutTypes.TracksDistance"/>.</summary>
        [Range(0, 1000)]
        public float? DistanceKm { get; set; }
    }
}
