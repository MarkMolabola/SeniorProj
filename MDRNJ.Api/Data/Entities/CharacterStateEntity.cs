using MDRNJ.Shared.Enums;
using System;

namespace MDRNJ.Api.Data.Entities
{
    public class CharacterStateEntity
    {
        public Guid CharacterId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public BodyType BodyType { get; set; } = BodyType.Athletic;
        public string OutfitSlot { get; set; } = "default";
        public string EquipmentSlot { get; set; } = "default";
        public string? AccessorySlot { get; set; }
        public DateTime LastEvolved { get; set; } = DateTime.UtcNow;

        // Navigation
        public UserEntity User { get; set; } = null!;
    }
}
