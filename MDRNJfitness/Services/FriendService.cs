using MDRNJ.Shared.DTOs.Friend;
using MDRNJfitness.Helpers.Extensions;
using MDRNJfitness.Services.Interfaces;

namespace MDRNJfitness.Services
{
    public class FriendService : IFriendService
    {
        private readonly HttpClient _http;

        public FriendService(HttpClient http) => _http = http;

        public Task<List<FriendDto>> GetFriendsAsync() =>
            _http.GetAsync<List<FriendDto>>("api/friends")!;

        public Task<List<FriendDto>> GetPendingRequestsAsync() =>
            _http.GetAsync<List<FriendDto>>("api/friends/requests")!;

        public Task<FriendDto> SendRequestAsync(string username) =>
            _http.PostAsync<FriendDto>("api/friends/request", new SendFriendRequestDto { Username = username })!;

        public Task<FriendDto> AcceptRequestAsync(Guid friendshipId) =>
            _http.PostAsync<FriendDto>($"api/friends/requests/{friendshipId}/accept", new { })!;

        public Task DeclineRequestAsync(Guid friendshipId) =>
            _http.PostAsync($"api/friends/requests/{friendshipId}/decline", new { });

        public Task RemoveFriendAsync(Guid userId) =>
            _http.DeleteAsync($"api/friends/{userId}");

        public Task<FriendPublicProfileDto> GetFriendProfileAsync(Guid userId) =>
            _http.GetAsync<FriendPublicProfileDto>($"api/friends/{userId}/profile")!;

        public Task BlockAsync(Guid userId) =>
            _http.PostAsync($"api/blocks/{userId}", new { });

        public Task UnblockAsync(Guid userId) =>
            _http.DeleteAsync($"api/blocks/{userId}");

        public Task<List<BlockedUserDto>> GetBlockedAsync() =>
            _http.GetAsync<List<BlockedUserDto>>("api/blocks")!;
    }
}
