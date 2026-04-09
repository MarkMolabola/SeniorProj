using MDRNJ.Shared.DTOs.Friend;

namespace MDRNJfitness.Services.Interfaces
{
    public interface IFriendService
    {
        Task<List<FriendDto>> GetFriendsAsync();
        Task<List<FriendDto>> GetPendingRequestsAsync();
        Task<FriendDto> SendRequestAsync(string username);
        Task<FriendDto> AcceptRequestAsync(Guid friendshipId);
        Task DeclineRequestAsync(Guid friendshipId);
        Task RemoveFriendAsync(Guid userId);
        Task<FriendPublicProfileDto> GetFriendProfileAsync(Guid userId);
    }
}
