using MDRNJ.Shared.Enums;
using System;

namespace MDRNJ.Api.Data.Entities
{
    public class FriendshipEntity
    {
        public Guid FriendshipId { get; set; } = Guid.NewGuid();
        public Guid RequesterId { get; set; }
        public Guid AddresseeId { get; set; }
        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public UserEntity Requester { get; set; } = null!;
        public UserEntity Addressee { get; set; } = null!;
    }
}
