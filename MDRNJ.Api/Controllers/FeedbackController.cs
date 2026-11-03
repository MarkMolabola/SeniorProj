using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Feedback;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDRNJ.Api.Controllers
{
    /// <summary>Bug reports and suggestions (players send; admins read and resolve).</summary>
    [ApiController]
    [Route("api/feedback")]
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedback;

        public FeedbackController(IFeedbackService feedback) => _feedback = feedback;

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Submit(SubmitFeedbackDto dto)
        {
            await _feedback.SubmitAsync(UserId, dto);
            return NoContent();
        }

        /// <summary>Admins: open feedback first, newest first.</summary>
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetInbox([FromQuery] bool includeResolved = false) =>
            Ok(await _feedback.GetInboxAsync(includeResolved));

        [HttpPost("{feedbackId:guid}/resolve")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Resolve(Guid feedbackId)
        {
            await _feedback.SetResolvedAsync(feedbackId, true);
            return NoContent();
        }

        [HttpPost("{feedbackId:guid}/reopen")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Reopen(Guid feedbackId)
        {
            await _feedback.SetResolvedAsync(feedbackId, false);
            return NoContent();
        }
    }
}
