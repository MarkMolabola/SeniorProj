using MDRNJ.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDRNJ.Api.Controllers
{
    /// <summary>Blocking players (see ModerationService for what a block does).</summary>
    [ApiController]
    [Route("api/blocks")]
    [Authorize]
    public class BlocksController : ControllerBase
    {
        private readonly IModerationService _moderation;

        public BlocksController(IModerationService moderation) => _moderation = moderation;

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetBlocked() =>
            Ok(await _moderation.GetBlockedAsync(UserId));

        [HttpPost("{blockedUserId:guid}")]
        public async Task<IActionResult> Block(Guid blockedUserId)
        {
            await _moderation.BlockAsync(UserId, blockedUserId);
            return NoContent();
        }

        [HttpDelete("{blockedUserId:guid}")]
        public async Task<IActionResult> Unblock(Guid blockedUserId)
        {
            await _moderation.UnblockAsync(UserId, blockedUserId);
            return NoContent();
        }
    }
}
