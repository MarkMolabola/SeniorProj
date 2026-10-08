using MDRNJ.Shared.Enums;
using System;

namespace MDRNJ.Api.Data.Entities
{
    public class CharacterStateEntity : MDRNJ.Shared.Game.IEquippedGear
    {
        public Guid CharacterId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        /// <summary>Chosen hero look; see MDRNJ.Shared.Game.HeroAvatars.</summary>
        public string AvatarId { get; set; } = "classic";
        public BodyType BodyType { get; set; } = BodyType.Athletic;
        public string OutfitSlot { get; set; } = "default";
        public string EquipmentSlot { get; set; } = "default";
        public string? AccessorySlot { get; set; }

        // Bought gear worn in each slot (GearCatalog item ids)
        public string? HeadGear { get; set; }
        public string? TopGear { get; set; }
        public string? BottomGear { get; set; }
        public string? ShoesGear { get; set; }
        public string? AccessoryGear { get; set; }
        public DateTime LastEvolved { get; set; } = DateTime.UtcNow;

        // Navigation
        public UserEntity User { get; set; } = null!;
    }
}
