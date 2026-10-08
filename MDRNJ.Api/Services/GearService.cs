using AutoMapper;
using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Character;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Game;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    public class GearService : IGearService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IGoalProgressService _goals;

        public GearService(AppDbContext db, IMapper mapper, IGoalProgressService goals)
        {
            _db = db;
            _mapper = mapper;
            _goals = goals;
        }

        public async Task<InventoryDto> GetInventoryAsync(Guid userId)
        {
            var stats = await _goals.GetOrCreateStatsAsync(userId);
            if (_db.Entry(stats).State == EntityState.Added)
                await _db.SaveChangesAsync();
            return await BuildAsync(userId, stats);
        }

        public async Task<InventoryDto> BuyAsync(Guid userId, string itemId)
        {
            var item = GearCatalog.Find(itemId)
                ?? throw new KeyNotFoundException("That item isn't in the shop.");

            if (await _db.UserGear.AnyAsync(g => g.UserId == userId && g.ItemId == item.Id))
                throw new InvalidOperationException($"You already own the {item.Name}.");

            var stats = await _goals.GetOrCreateStatsAsync(userId);
            if (stats.Gold < item.Price)
                throw new InvalidOperationException($"Not enough gold: the {item.Name} costs {item.Price}, you have {stats.Gold}.");

            stats.Gold -= item.Price;
            _db.UserGear.Add(new UserGearEntity { UserId = userId, ItemId = item.Id, PricePaid = item.Price });
            await _db.SaveChangesAsync();
            return await BuildAsync(userId, stats);
        }

        public async Task<InventoryDto> EquipAsync(Guid userId, string slot, string? itemId)
        {
            if (!Enum.TryParse<GearSlot>(slot, true, out var gearSlot) || !Enum.IsDefined(gearSlot))
                throw new ArgumentException($"Unknown gear slot: {slot}");

            var character = await _db.CharacterStates.FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new KeyNotFoundException("Character not found.");

            if (itemId is not null)
            {
                var item = GearCatalog.Find(itemId)
                    ?? throw new KeyNotFoundException("That item doesn't exist.");
                if (item.Slot != gearSlot)
                    throw new ArgumentException($"The {item.Name} is worn on {item.Slot}, not {gearSlot}.");
                if (!await _db.UserGear.AnyAsync(g => g.UserId == userId && g.ItemId == item.Id))
                    throw new InvalidOperationException($"Buy the {item.Name} in the shop first.");
            }

            character.Set(gearSlot, itemId);
            await _db.SaveChangesAsync();
            return await BuildAsync(userId, await _goals.GetOrCreateStatsAsync(userId), character);
        }

        private async Task<InventoryDto> BuildAsync(Guid userId, GameStatsEntity stats, CharacterStateEntity? character = null)
        {
            character ??= await _db.CharacterStates.FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new KeyNotFoundException("Character not found.");

            var owned = await _db.UserGear
                .Where(g => g.UserId == userId)
                .OrderBy(g => g.PurchasedAt)
                .Select(g => g.ItemId)
                .ToListAsync();

            return new InventoryDto
            {
                Gold = stats.Gold,
                OwnedItemIds = owned,
                Character = _mapper.Map<CharacterStateDto>(character)
            };
        }
    }
}
