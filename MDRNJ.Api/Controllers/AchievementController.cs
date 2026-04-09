using MDRNJ.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDRNJ.Api.Controllers
{
    [ApiController]
    [Route("api/achievements")]
    [Authorize]
    public class AchievementController : ControllerBase
    {
        private readonly IAchievementService _achievements;

        public AchievementController(IAchievementService achievements) => _achievements = achievements;

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _achievements.GetAllAsync(UserId));

        [HttpGet("unlocked")]
        public async Task<IActionResult> GetUnlocked() => Ok(await _achievements.GetUnlockedAsync(UserId));

        [HttpPost("check")]
        public async Task<IActionResult> Check() => Ok(await _achievements.CheckAndUnlockAsync(UserId));
    }
}
