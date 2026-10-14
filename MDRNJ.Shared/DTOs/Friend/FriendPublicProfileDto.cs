using MDRNJ.Shared.DTOs.Workout;
using System;
using System.Collections.Generic;

namespace MDRNJ.Shared.DTOs.Friend
{
    public class FriendPublicProfileDto : MDRNJ.Shared.Game.IEquippedGear
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = null!;
        public string? AvatarUrl { get; set; }

        /// <summary>How the viewer relates to this user: Self, Friends, RequestSent, RequestReceived or None.</summary>
        public string Relationship { get; set; } = FriendRelationship.None;

        /// <summary>The pending request to accept when Relationship is RequestReceived.</summary>
        public Guid? PendingFriendshipId { get; set; }

        /// <summary>The viewer has blocked this player.</summary>
        public bool IsBlocked { get; set; }

        // Game stats
        public int CurrentLevel { get; set; }
        public int TotalXP { get; set; }
        public int TotalWorkouts { get; set; }
        public int CurrentStreak { get; set; }

        // Dominant activity (maps to pixel avatar type)
        public string DominantSport { get; set; } = "None";
        public string SpriteState { get; set; } = "default";
        public string AvatarId { get; set; } = "classic";
        public string? HeadGear { get; set; }
        public string? TopGear { get; set; }
        public string? BottomGear { get; set; }
        public string? ShoesGear { get; set; }
        public string? AccessoryGear { get; set; }

        // Latest 3 completed workouts
        public List<WorkoutDto> RecentWorkouts { get; set; } = new();
    }
}
