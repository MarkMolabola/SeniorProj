using System.ComponentModel.DataAnnotations;

namespace MDRNJ.Shared.DTOs.Friend
{
    public class SendFriendRequestDto
    {
        [Required]
        public required string Username { get; set; }
    }
}
