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

        [HttpGet("my")]
        public async Task<IActionResult> GetMyChallenges() =>
            Ok(await _challenges.GetMyAsync(UserId));

        [HttpPost("{challengeId:guid}/accept")]
        public async Task<IActionResult> Accept(Guid challengeId) =>
            Ok(await _challenges.AcceptAsync(UserId, challengeId));

        [HttpPost("{challengeId:guid}/decline")]
        public async Task<IActionResult> Decline(Guid challengeId) =>
            Ok(await _challenges.DeclineAsync(UserId, challengeId));

        [HttpPost("{challengeId:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid challengeId) =>
            Ok(await _challenges.CancelAsync(UserId, challengeId));

        [HttpPost("{challengeId:guid}/attempts")]
        public async Task<IActionResult> SubmitAttempt(Guid challengeId, [FromBody] SubmitAttemptDto dto) =>
            Ok(await _challenges.SubmitAttemptAsync(UserId, challengeId, dto.Value, dto.ProofPath));

        [HttpPost("{challengeId:guid}/proof-upload")]
        public async Task<IActionResult> CreateProofUpload(Guid challengeId, [FromBody] ProofUploadRequestDto dto) =>
            Ok(await _challenges.CreateProofUploadAsync(UserId, challengeId, dto.ContentType));

        [HttpGet("{challengeId:guid}/attempts/{attemptId:guid}/proof")]
        public async Task<IActionResult> GetProofLink(Guid challengeId, Guid attemptId) =>
            Ok(await _challenges.GetProofLinkAsync(UserId, challengeId, attemptId));

        [HttpPost("{challengeId:guid}/approve")]
        public async Task<IActionResult> Approve(Guid challengeId) =>
            Ok(await _challenges.ApproveAsync(UserId, challengeId));

        [HttpPost("{challengeId:guid}/dispute")]
        public async Task<IActionResult> Dispute(Guid challengeId, [FromBody] DisputeChallengeDto dto) =>
            Ok(await _challenges.DisputeAsync(UserId, challengeId, dto.Reason));

        [HttpPost("{challengeId:guid}/seen")]
        public async Task<IActionResult> MarkSeen(Guid challengeId)
        {
            await _challenges.MarkResultSeenAsync(UserId, challengeId);
            return NoContent();
        }
    }
}
