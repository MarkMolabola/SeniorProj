using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Friend;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDRNJ.Api.Controllers
{
    [ApiController]
    [Route("api/friends")]
    [Authorize]
    public class FriendsController : ControllerBase
    {
        private readonly IFriendService _friends;

        public FriendsController(IFriendService friends) => _friends = friends;

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetFriends() =>
            Ok(await _friends.GetFriendsAsync(UserId));

        [HttpGet("{friendUserId:guid}/profile")]
        public async Task<IActionResult> GetFriendProfile(Guid friendUserId) =>
            Ok(await _friends.GetFriendProfileAsync(UserId, friendUserId));

        [HttpGet("requests")]
        public async Task<IActionResult> GetPendingRequests() =>
            Ok(await _friends.GetPendingRequestsAsync(UserId));

        [HttpPost("request")]
        public async Task<IActionResult> SendRequest([FromBody] SendFriendRequestDto dto) =>
            Ok(await _friends.SendFriendRequestAsync(UserId, dto));

        [HttpPost("requests/{friendshipId:guid}/accept")]
        public async Task<IActionResult> Accept(Guid friendshipId) =>
            Ok(await _friends.AcceptFriendRequestAsync(UserId, friendshipId));

        [HttpPost("requests/{friendshipId:guid}/decline")]
        public async Task<IActionResult> Decline(Guid friendshipId)
        {
            await _friends.DeclineFriendRequestAsync(UserId, friendshipId);
            return NoContent();
        }

        [HttpDelete("{friendUserId:guid}")]
        public async Task<IActionResult> Remove(Guid friendUserId)
        {
            await _friends.RemoveFriendAsync(UserId, friendUserId);
            return NoContent();
        }
    }
}
