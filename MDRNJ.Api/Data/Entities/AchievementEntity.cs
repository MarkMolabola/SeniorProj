using MDRNJ.Shared.Enums;
using System;
using System.Collections.Generic;

namespace MDRNJ.Api.Data.Entities
{
    public class AchievementEntity
    {
        public Guid AchievementId { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string BadgeImageUrl { get; set; } = null!;
        public int XPReward { get; set; }
        public AchievementRarity Rarity { get; set; }

        // Navigation
        public ICollection<UserAchievementEntity> UserAchievements { get; set; } = new List<UserAchievementEntity>();
    }
}
