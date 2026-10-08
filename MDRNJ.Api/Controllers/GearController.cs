using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Game;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDRNJ.Api.Controllers
{
    /// <summary>Gold, the shop, and equipping gear. The catalog itself is MDRNJ.Shared.Game.GearCatalog.</summary>
    [ApiController]
    [Route("api/gear")]
    [Authorize]
    public class GearController : ControllerBase
    {
        private readonly IGearService _gear;

        public GearController(IGearService gear) => _gear = gear;

        private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetInventory() =>
            Ok(await _gear.GetInventoryAsync(UserId));

        [HttpPost("{itemId}/buy")]
        public async Task<IActionResult> Buy(string itemId) =>
            Ok(await _gear.BuyAsync(UserId, itemId));

        [HttpPut("equip")]
        public async Task<IActionResult> Equip([FromBody] EquipGearDto dto) =>
            Ok(await _gear.EquipAsync(UserId, dto.Slot, dto.ItemId));
    }
}
