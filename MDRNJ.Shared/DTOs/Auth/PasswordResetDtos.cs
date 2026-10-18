using System.ComponentModel.DataAnnotations;

namespace MDRNJ.Shared.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }

    public class ResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Code must be 6 digits.")]
        public required string Code { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public required string NewPassword { get; set; }
    }
}
