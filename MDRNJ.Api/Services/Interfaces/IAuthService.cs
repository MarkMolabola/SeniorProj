using MDRNJ.Shared.DTOs.Auth;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailDto dto);
        Task ResendVerificationAsync(ResendVerificationDto dto);
        Task<AuthResponseDto> GoogleSignInAsync(ClaimsPrincipal googleUser);

        /// <summary>Swaps a valid refresh token for a new access + refresh token pair.</summary>
        Task<AuthResponseDto> RefreshAsync(string refreshToken);

        /// <summary>Revokes a refresh token (signs that device out).</summary>
        Task LogoutAsync(string refreshToken);
    }
}
