using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDRNJ.Shared.DTOs.Character;

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

        /// <summary>Total distance logged in this session, or null if none.</summary>
        public float? DistanceKm => Exercises.Any(e => e.DistanceKm > 0) ? Exercises.Sum(e => e.DistanceKm ?? 0) : null;

        /// <summary>Set when completing the workout changed the character.</summary>
        public CharacterProgressDto? CharacterProgress { get; set; }

        /// <summary>Set when completing the workout: XP, level and quests / challenges it completed.</summary>
        public MDRNJ.Shared.DTOs.Game.AccountProgressDto? AccountProgress { get; set; }
    }
}
