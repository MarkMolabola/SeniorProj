using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Chat;
using MDRNJ.Shared.Game;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDRNJ.Api.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chat;
        private readonly IModerationService _moderation;

        public ChatController(IChatService chat, IModerationService moderation)
        {
            _chat = chat;
            _moderation = moderation;
        }

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>Global first, then the user's groups and direct chats, most recent first.</summary>
        [HttpGet("channels")]
        public async Task<IActionResult> GetChannels() =>
            Ok(await _chat.GetChannelsAsync(UserId));

        /// <summary>Latest messages (oldest first); pass <c>before</c> to page further back.</summary>
        [HttpGet("channels/{channelId:guid}/messages")]
        public async Task<IActionResult> GetMessages(Guid channelId, [FromQuery] DateTime? before, [FromQuery] int take = ChatRules.PageSize) =>
            Ok(await _chat.GetMessagesAsync(UserId, channelId, before, take));

        [HttpPost("channels/{channelId:guid}/messages")]
        public async Task<IActionResult> SendMessage(Guid channelId, [FromBody] SendChatMessageDto dto) =>
            Ok(await _chat.SendMessageAsync(UserId, channelId, dto));

        [HttpPost("messages/{messageId:guid}/report")]
        public async Task<IActionResult> Report(Guid messageId, [FromBody] ReportChatMessageDto dto)
        {
            await _moderation.ReportMessageAsync(UserId, messageId, dto);
            return NoContent();
        }

        /// <summary>Your own message, or anyone's for admins.</summary>
        [HttpDelete("messages/{messageId:guid}")]
        public async Task<IActionResult> DeleteMessage(Guid messageId)
        {
            await _moderation.DeleteMessageAsync(UserId, messageId);
            return NoContent();
        }

        /// <summary>Admins: reported messages to review.</summary>
        [HttpGet("reports")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetReports() =>
            Ok(await _moderation.GetReportsAsync());

        /// <summary>Admins: the message is fine; clear its reports and show it again.</summary>
        [HttpPost("reports/{messageId:guid}/dismiss")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DismissReports(Guid messageId)
        {
            await _moderation.DismissReportsAsync(messageId);
            return NoContent();
        }

        [HttpPost("channels/{channelId:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid channelId)
        {
            await _chat.MarkReadAsync(UserId, channelId);
            return NoContent();
        }

        [HttpGet("channels/{channelId:guid}/members")]
        public async Task<IActionResult> GetMembers(Guid channelId) =>
            Ok(await _chat.GetMembersAsync(UserId, channelId));

        /// <summary>Finds or starts the direct chat with a friend.</summary>
        [HttpPost("direct/{otherUserId:guid}")]
        public async Task<IActionResult> OpenDirect(Guid otherUserId) =>
            Ok(await _chat.OpenDirectAsync(UserId, otherUserId));

        [HttpPost("groups")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateChatGroupDto dto) =>
            Ok(await _chat.CreateGroupAsync(UserId, dto));

        [HttpPost("groups/{channelId:guid}/members")]
        public async Task<IActionResult> AddMembers(Guid channelId, [FromBody] AddChatMembersDto dto) =>
            Ok(await _chat.AddMembersAsync(UserId, channelId, dto));

        [HttpPost("groups/{channelId:guid}/leave")]
        public async Task<IActionResult> Leave(Guid channelId)
        {
            await _chat.LeaveGroupAsync(UserId, channelId);
            return NoContent();
        }
    }
}
