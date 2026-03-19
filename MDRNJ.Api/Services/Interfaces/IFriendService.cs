using MDRNJ.Shared.DTOs.Friend;
using MDRNJ.Shared.DTOs.Workout;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services.Interfaces
{
    public interface IFriendService
    {
        Task<FriendDto> SendFriendRequestAsync(Guid requesterId, SendFriendRequestDto dto);
        Task<FriendDto> AcceptFriendRequestAsync(Guid addresseeId, Guid friendshipId);
        Task DeclineFriendRequestAsync(Guid addresseeId, Guid friendshipId);
        Task RemoveFriendAsync(Guid userId, Guid friendUserId);
        Task<List<FriendDto>> GetFriendsAsync(Guid userId);
        Task<List<FriendDto>> GetPendingRequestsAsync(Guid userId);
        Task<FriendPublicProfileDto> GetFriendProfileAsync(Guid requestingUserId, Guid friendUserId);
    }
}
