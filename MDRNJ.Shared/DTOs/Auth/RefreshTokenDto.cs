using System.ComponentModel.DataAnnotations;

namespace MDRNJ.Shared.DTOs.Auth
{
    /// <summary>Body for /api/auth/refresh and /api/auth/logout.</summary>
    public class RefreshTokenDto
    {
        [Required]
        [MaxLength(200)]
        public required string RefreshToken { get; set; }
    }
}
