using MDRNJ.Api.Services.Interfaces;
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

        [HttpGet("activity-score")]
        public async Task<IActionResult> GetActivityScore() => Ok(await _character.GetActivityScoreAsync(UserId));

        [HttpPost("activity-score/recalculate")]
        public async Task<IActionResult> RecalculateActivityScore() =>
            Ok(await _character.RecalculateActivityScoreAsync(UserId));
    }
}
