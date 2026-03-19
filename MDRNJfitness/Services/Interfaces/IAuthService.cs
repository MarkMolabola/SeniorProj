using MDRNJ.Shared.DTOs.Auth;
using MDRNJ.Shared.DTOs.User;

namespace MDRNJfitness.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task LogoutAsync();
        Task<UserDto?> GetCurrentUserAsync();
    }
}
