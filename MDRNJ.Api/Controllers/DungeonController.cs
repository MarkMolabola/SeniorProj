using MDRNJ.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDRNJ.Api.Controllers
{
    /// <summary>The passive dungeon on the Home tab: see gold building up, and collect it.</summary>
    [ApiController]
    [Route("api/dungeon")]
    [Authorize]
    public class DungeonController : ControllerBase
    {
        private readonly IDungeonService _dungeon;

        public DungeonController(IDungeonService dungeon) => _dungeon = dungeon;

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> Get() =>
            Ok(await _dungeon.GetAsync(UserId));

        [HttpPost("collect")]
        public async Task<IActionResult> Collect() =>
            Ok(await _dungeon.CollectAsync(UserId));
    }
}
