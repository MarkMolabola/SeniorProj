using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MDRNJ.Shared.DTOs.Auth
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public required string Email { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public required string Password { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(30)]
        public required string Username { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public required string DisplayName { get; set; }
    }



}
