using MDRNJ.Shared.DTOs.Character;

namespace MDRNJ.Shared.DTOs.Game
{
    /// <summary>The player's gold, the gear they own, and what their hero is wearing.</summary>
    public class InventoryDto
    {
        public int Gold { get; set; }

        /// <summary>Admins shop for free; the app shows their gold as ∞.</summary>
        public bool UnlimitedGold { get; set; }
        public List<string> OwnedItemIds { get; set; } = new();
        public CharacterStateDto Character { get; set; } = null!;
    }

    public class EquipGearDto
    {
        /// <summary>A <see cref="MDRNJ.Shared.Game.GearSlot"/> name, e.g. "Head".</summary>
        public string Slot { get; set; } = null!;

        /// <summary>Item to wear, or null to take the slot off.</summary>
        public string? ItemId { get; set; }
    }
}
