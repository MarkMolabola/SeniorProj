using MDRNJ.Shared.DTOs.Auth;
using MDRNJ.Shared.DTOs.User;
using MDRNJfitness.Data;
using MDRNJfitness.Helpers.Extensions;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly UserRepo _userRepo;

        public AuthService(HttpClient http, UserRepo userRepo)
        {
            _http = http;
            _userRepo = userRepo;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var response = await _http.PostAsync<AuthResponseDto>("api/auth/login", dto);
            if (response is not null)
            {
                await SecureStorage.SetAsync("access_token", response.AccessToken);
                if (response.User is not null)
                    await _userRepo.CacheCurrentUserAsync(response.User);
            }
            return response!;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var response = await _http.PostAsync<AuthResponseDto>("api/auth/register", dto);
            if (response is not null)
            {
                await SecureStorage.SetAsync("access_token", response.AccessToken);
                if (response.User is not null)
                    await _userRepo.CacheCurrentUserAsync(response.User);
            }
            return response!;
        }

        public async Task LogoutAsync()
        {
            SecureStorage.Remove("access_token");
            await _userRepo.ClearAsync();
        }

        public Task<UserDto?> GetCurrentUserAsync() =>
            _userRepo.GetCachedCurrentUserAsync();
    }
}
