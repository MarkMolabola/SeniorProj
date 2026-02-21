using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MDRNJ.Shared.DTOs.Friend
{
    public class SendFriendRequestDto
    {
        [Required]
        public required Guid AddresseeId { get; set; }
    }
}
