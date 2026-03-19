using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.DTOs.Game
{
    public class AchievementDto
    {
        public Guid AchievementId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string BadgeImageUrl { get; set; }
        public int XPReward { get; set; }
        public string Rarity { get; set; }
        public DateTime? UnlockedAt { get; set; }
    }
}
