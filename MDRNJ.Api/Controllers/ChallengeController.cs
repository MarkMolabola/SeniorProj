using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Challenge;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDRNJ.Api.Controllers
{
    [ApiController]
    [Route("api/challenges")]
    [Authorize]
    public class ChallengeController : ControllerBase
    {
        private readonly IChallengeService _challenges;

        public ChallengeController(IChallengeService challenges) => _challenges = challenges;

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateChallengeDto dto) =>
            Ok(await _challenges.CreateAsync(UserId, dto));

        [HttpPost("{challengeId:guid}/join")]
        public async Task<IActionResult> Join(Guid challengeId) =>
            Ok(await _challenges.JoinAsync(UserId, challengeId));

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveChallenges() =>
            Ok(await _challenges.GetActiveAsync(UserId));

        [HttpGet("my")]
        public async Task<IActionResult> GetMyChallenges() =>
            Ok(await _challenges.GetUserChallengesAsync(UserId));

        [HttpPut("{challengeId:guid}/progress")]
        public async Task<IActionResult> UpdateProgress(Guid challengeId, [FromQuery] float progress) =>
            Ok(await _challenges.UpdateProgressAsync(UserId, challengeId, progress));
    }
}
