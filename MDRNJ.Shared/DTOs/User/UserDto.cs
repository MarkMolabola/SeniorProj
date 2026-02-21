using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.DTOs.User
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime DateJoined { get; set; }
    }
}
