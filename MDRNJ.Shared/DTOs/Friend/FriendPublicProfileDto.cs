using MDRNJ.Shared.DTOs.Workout;
using System;
using System.Collections.Generic;

namespace MDRNJ.Shared.DTOs.Friend
{
    public class FriendPublicProfileDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = null!;
        public string? AvatarUrl { get; set; }

        // Game stats
        public int CurrentLevel { get; set; }
        public int TotalXP { get; set; }
        public int TotalWorkouts { get; set; }
        public int CurrentStreak { get; set; }

        // Dominant activity (maps to pixel avatar type)
        public string DominantSport { get; set; } = "None";
        public string SpriteState { get; set; } = "default";

        // Latest 3 completed workouts
        public List<WorkoutDto> RecentWorkouts { get; set; } = new();
    }
}
