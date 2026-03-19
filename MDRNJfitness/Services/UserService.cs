using MDRNJ.Shared.DTOs.User;
using MDRNJfitness.Helpers.Extensions;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _http;

        public UserService(HttpClient http) => _http = http;

        public Task<UserDto> GetCurrentUserAsync() =>
            _http.GetAsync<UserDto>("api/users/me")!;

        public Task<UserProfileDto> GetProfileAsync() =>
            _http.GetAsync<UserProfileDto>("api/users/me/profile")!;

        public Task<UserProfileDto> UpdateProfileAsync(UpdateUserProfileDto dto) =>
            _http.PutAsync<UserProfileDto>("api/users/me/profile", dto)!;
    }
}
