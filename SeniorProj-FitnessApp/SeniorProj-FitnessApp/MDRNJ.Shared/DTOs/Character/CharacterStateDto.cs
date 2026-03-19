using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.DTOs.Character
{
    public class CharacterStateDto
    {
        public Guid CharacterId { get; set; }
        public Guid UserId { get; set; }
        public int BodyType { get; set; }
        public string OutfitSlot { get; set; }
        public string EquipmentSlot { get; set; }
        public string? AccessorySlot { get; set; }
        public DateTime LastEvolved { get; set; }
        public SpriteConfigDto SpriteConfig { get; set; }
    }
}
