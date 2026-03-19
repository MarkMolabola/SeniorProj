using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MDRNJ.Shared.DTOs.Auth
{
    public class LoginDto
    {
      
        [Required]
        [EmailAddress]           // also validates email format
        public required string Email { get; set; }

        [Required]
        [MinLength(8)]           // minimum password length
        public required string Password { get; set; }

    }
}
