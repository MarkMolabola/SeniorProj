using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.DTOs.Character
{
    public class SpriteConfigDto
    {
        public string BodySpritePath { get; set; }
        public string OutfitSpritePath { get; set; }
        public string EquipmentSpritePath { get; set; }
        public string? AccessorySpritePath { get; set; }
    }
}
