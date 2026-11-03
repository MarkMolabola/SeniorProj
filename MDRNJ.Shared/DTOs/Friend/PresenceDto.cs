using System;

namespace MDRNJ.Shared.DTOs.Friend
{
    /// <summary>Pushed to a user's friends as "PresenceChanged" whenever their status light changes.</summary>
    public class PresenceDto
    {
        public Guid UserId { get; set; }

        /// <summary>A <see cref="MDRNJ.Shared.Enums.PresenceStatus"/> name.</summary>
        public string Presence { get; set; } = "Offline";

        /// <summary>The workout type name while <see cref="Presence"/> is WorkingOut.</summary>
        public string? WorkoutType { get; set; }
    }
}
