using MDRNJ.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDRNJ.Api.Controllers
{
    [ApiController]
    [Route("api/leaderboard")]
    [Authorize]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboard;

        public LeaderboardController(ILeaderboardService leaderboard) => _leaderboard = leaderboard;

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetTop([FromQuery] int count = 50) =>
            Ok(await _leaderboard.GetTopAsync(count));

        [HttpGet("me")]
        public async Task<IActionResult> GetMyRank() =>
            Ok(await _leaderboard.GetUserRankAsync(UserId));
    }
}
