using AutoMapper;
using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Character;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Enums;
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

        /// <summary>Buys a gear item, or a special hero (by its HeroAvatars id).</summary>
        public async Task<InventoryDto> BuyAsync(Guid userId, string itemId)
        {
            var (id, name, listPrice) = GearCatalog.Find(itemId) is { DropOnly: false } item ? (item.Id, item.Name, item.Price)
                : HeroAvatars.Find(itemId) is { IsSpecial: true } hero ? (hero.Id, hero.Name, hero.Price)
                : throw new KeyNotFoundException("That item isn't in the shop.");

            if (await _db.UserGear.AnyAsync(g => g.UserId == userId && g.ItemId == id))
                throw new InvalidOperationException($"You already own the {name}.");

            var stats = await _goals.GetOrCreateStatsAsync(userId);
            var price = await IsAdminAsync(userId) ? 0 : listPrice;
            if (stats.Gold < price)
                throw new InvalidOperationException($"Not enough gold: the {name} costs {listPrice}, you have {stats.Gold}.");

            _goals.SpendGold(stats, price, GoldReason.Purchase, name);
            _db.UserGear.Add(new UserGearEntity { UserId = userId, ItemId = id, PricePaid = price });
            await _db.SaveChangesAsync();
            return await BuildAsync(userId, stats);
        }

        public async Task<InventoryDto> EquipAsync(Guid userId, string slot, string? itemId)
        {
            if (!Enum.TryParse<GearSlot>(slot, true, out var gearSlot) || !Enum.IsDefined(gearSlot))
                throw new ArgumentException($"Unknown gear slot: {slot}");

            var character = await _db.CharacterStates.FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new KeyNotFoundException("Character not found.");

            // Special heroes come dressed; gear goes on the Classic Hero (and is kept for it meanwhile)
            if (HeroAvatars.Find(character.AvatarId) is { IsSpecial: true } hero)
                throw new InvalidOperationException(
                    $"The {hero.Name}'s outfit comes with the hero. Switch to the Classic Hero to wear gear.");

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

        /// <summary>Admins have unlimited gold: shop items are free for them.</summary>
        private Task<bool> IsAdminAsync(Guid userId) =>
            _db.Users.Where(u => u.UserId == userId).Select(u => u.IsAdmin).FirstOrDefaultAsync();

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
                UnlimitedGold = await IsAdminAsync(userId),
                OwnedItemIds = owned,
                Character = _mapper.Map<CharacterStateDto>(character)
            };
        }
    }
}
