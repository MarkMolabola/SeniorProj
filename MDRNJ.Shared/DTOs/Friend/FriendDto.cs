using System;

namespace MDRNJ.Shared.DTOs.Friend
{
    public class FriendDto
    {
        public Guid FriendshipId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        /// <summary>A <see cref="MDRNJ.Shared.Enums.PresenceStatus"/> name; filled for the friends list.</summary>
        public string Presence { get; set; } = "Offline";

        /// <summary>The workout type name while <see cref="Presence"/> is WorkingOut.</summary>
        public string? PresenceWorkoutType { get; set; }
    }
}
