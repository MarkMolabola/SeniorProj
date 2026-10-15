using System.ComponentModel.DataAnnotations;

namespace MDRNJ.Shared.DTOs.Workout
{
    public class EndWorkoutDto
    {
        /// <summary>Distance covered, for types where <see cref="MDRNJ.Shared.Game.WorkoutTypes.TracksDistance"/>.</summary>
        [Range(0, 1000)]
        public float? DistanceKm { get; set; }
    }
}
