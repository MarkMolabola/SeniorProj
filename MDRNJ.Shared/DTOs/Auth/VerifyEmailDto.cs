using System.ComponentModel.DataAnnotations;

namespace MDRNJ.Shared.DTOs.Auth
{
    public class VerifyEmailDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be 6 digits.")]
        public required string Code { get; set; }
    }
}
