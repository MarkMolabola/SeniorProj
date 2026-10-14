using System;

namespace MDRNJ.Api.Data.Entities
{
    /// <summary>
    /// Blocker no longer sees Blocked's chat messages; neither can friend, message or
    /// invite the other while it stands.
    /// </summary>
    public class UserBlockEntity
    {
        public Guid BlockerId { get; set; }
        public Guid BlockedId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public UserEntity Blocker { get; set; } = null!;
        public UserEntity Blocked { get; set; } = null!;
    }
}
