using MDRNJ.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDRNJ.Api.Controllers
{
    [ApiController]
    [Route("api/game")]
    [Authorize]
    public class GameController : ControllerBase
    {
        private readonly IGameService _game;

        public GameController(IGameService game) => _game = game;

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats() => Ok(await _game.GetStatsAsync(UserId));

        [HttpGet("xp/history")]
        public async Task<IActionResult> GetXPHistory() => Ok(await _game.GetXPHistoryAsync(UserId));

        [HttpGet("gold/history")]
        public async Task<IActionResult> GetGoldHistory([FromQuery] int count = 50) =>
            Ok(await _game.GetGoldHistoryAsync(UserId, count));
    }
}
