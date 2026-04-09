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
    }
}
