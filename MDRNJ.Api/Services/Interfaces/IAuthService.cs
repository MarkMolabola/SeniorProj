using MDRNJ.Shared.DTOs.Auth;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }
}
