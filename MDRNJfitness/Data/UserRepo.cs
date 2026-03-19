using MDRNJ.Shared.DTOs.User;

namespace MDRNJfitness.Data
{
    public class UserRepo
    {
        private readonly CacheRepo _cache;
        private const string CurrentUserKey = "current_user";
        private const string ProfileKey = "current_profile";

        public UserRepo(CacheRepo cache) => _cache = cache;

        public Task CacheCurrentUserAsync(UserDto user) =>
            _cache.SetAsync(CurrentUserKey, user);

        public Task<UserDto?> GetCachedCurrentUserAsync() =>
            _cache.GetAsync<UserDto>(CurrentUserKey);

        public Task CacheProfileAsync(UserProfileDto profile) =>
            _cache.SetAsync(ProfileKey, profile);

        public Task<UserProfileDto?> GetCachedProfileAsync() =>
            _cache.GetAsync<UserProfileDto>(ProfileKey);

        public async Task ClearAsync()
        {
            await _cache.RemoveAsync(CurrentUserKey);
            await _cache.RemoveAsync(ProfileKey);
        }
    }
}
