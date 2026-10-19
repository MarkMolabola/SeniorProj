using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Character;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDRNJ.Api.Controllers
{
    [ApiController]
    [Route("api/character")]
    [Authorize]
    public class CharacterController : ControllerBase
    {
        private readonly ICharacterService _character;

        public CharacterController(ICharacterService character) => _character = character;

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _character.GetCharacterAsync(UserId));

        [HttpPut("avatar")]
        public async Task<IActionResult> SetAvatar([FromBody] SetAvatarDto dto) =>
            Ok(await _character.SetAvatarAsync(UserId, dto.AvatarId));

        [HttpGet("activity-score")]
        public async Task<IActionResult> GetActivityScore() => Ok(await _character.GetActivityScoreAsync(UserId));

        [HttpPost("activity-score/recalculate")]
        public async Task<IActionResult> RecalculateActivityScore() =>
            Ok(await _character.RecalculateActivityScoreAsync(UserId));

        [HttpPost("stat-points")]
        public async Task<IActionResult> SpendStatPoints([FromBody] SpendStatPointsDto dto) =>
            Ok(await _character.SpendStatPointsAsync(UserId, dto.Stat, dto.Points));
    }
}
