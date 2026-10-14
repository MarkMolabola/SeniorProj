using MDRNJ.Shared.DTOs.Auth;
using MDRNJ.Shared.DTOs.User;

namespace MDRNJfitness.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<RegisterResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailDto dto);
        Task ResendVerificationAsync(string email);

        /// <summary>Returns false if the user cancelled the Google sign-in screen.</summary>
        Task<bool> SignInWithGoogleAsync();

        Task LogoutAsync();

        /// <summary>Permanently deletes the account on the server, then clears the local session.</summary>
        Task DeleteAccountAsync();

        /// <summary>Signs back in with the saved refresh token (app launch). False if there's no valid session.</summary>
        Task<bool> TryResumeSessionAsync();
        Task<UserDto?> GetCurrentUserAsync();
    }
}
