using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDRNJ.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _users;

        public UserController(IUserService users) => _users = users;

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("me")]
        public async Task<IActionResult> GetMe() => Ok(await _users.GetByIdAsync(UserId));

        [HttpGet("me/profile")]
        public async Task<IActionResult> GetProfile() => Ok(await _users.GetProfileAsync(UserId));

        [HttpPut("me/profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto dto) =>
            Ok(await _users.UpdateProfileAsync(UserId, dto));

        /// <summary>The player agreed to the current Terms of Service and Privacy Policy.</summary>
        [HttpPost("me/accept-terms")]
        public async Task<IActionResult> AcceptTerms()
        {
            await _users.AcceptTermsAsync(UserId);
            return NoContent();
        }

        [HttpDelete("me")]
        public async Task<IActionResult> DeleteAccount()
        {
            await _users.DeleteAccountAsync(UserId);
            return NoContent();
        }
    }
}
