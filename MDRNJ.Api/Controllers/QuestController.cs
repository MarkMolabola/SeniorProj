using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Quest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MDRNJ.Api.Controllers
{
    [ApiController]
    [Route("api/quests")]
    [Authorize]
    public class QuestController : ControllerBase
    {
        private readonly IQuestService _quests;

        public QuestController(IQuestService quests) => _quests = quests;

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ── User endpoints ──────────────────────────────────

        [HttpGet("my")]
        public async Task<IActionResult> GetMyQuests() =>
            Ok(await _quests.GetMyQuestsAsync(UserId));

        [HttpPut("{questId:guid}/progress")]
        public async Task<IActionResult> UpdateProgress(Guid questId, [FromQuery] float progress) =>
            Ok(await _quests.UpdateProgressAsync(UserId, questId, progress));

        // ── Admin endpoints ─────────────────────────────────

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll() =>
            Ok(await _quests.GetAllAsync());

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateQuestDto dto) =>
            Ok(await _quests.CreateAsync(dto));

        [HttpPut("{questId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid questId, [FromBody] UpdateQuestDto dto) =>
            Ok(await _quests.UpdateAsync(questId, dto));

        [HttpDelete("{questId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid questId)
        {
            await _quests.DeleteAsync(questId);
            return NoContent();
        }

        [HttpPost("{questId:guid}/send-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendToAll(Guid questId)
        {
            await _quests.SendToAllUsersAsync(questId);
            return Ok();
        }

        [HttpPost("{questId:guid}/send/{userId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendToUser(Guid questId, Guid userId)
        {
            await _quests.SendToUserAsync(questId, userId);
            return Ok();
        }
    }
}
