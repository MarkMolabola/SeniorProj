using System.ComponentModel.DataAnnotations;

namespace MDRNJ.Shared.DTOs.Auth
{
    public class ResendVerificationDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}
