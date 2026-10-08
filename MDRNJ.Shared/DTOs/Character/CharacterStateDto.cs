using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.DTOs.Character
{
    public class CharacterStateDto : MDRNJ.Shared.Game.IEquippedGear
    {
        public Guid CharacterId { get; set; }
        public Guid UserId { get; set; }
        public string AvatarId { get; set; } = "classic";
        public int BodyType { get; set; }
        public string OutfitSlot { get; set; }
        public string EquipmentSlot { get; set; }
        public string? AccessorySlot { get; set; }
        public string? HeadGear { get; set; }
        public string? TopGear { get; set; }
        public string? BottomGear { get; set; }
        public string? ShoesGear { get; set; }
        public string? AccessoryGear { get; set; }
        public DateTime LastEvolved { get; set; }
        public SpriteConfigDto SpriteConfig { get; set; }
    }
}
