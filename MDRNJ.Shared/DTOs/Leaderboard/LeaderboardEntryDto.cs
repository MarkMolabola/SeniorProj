using MDRNJ.Shared.DTOs.Character;
using System;
using System.Collections.Generic;
using System.Text;

namespace MDRNJ.Shared.DTOs.Leaderboard
{
    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public int CurrentLevel { get; set; }
        public int TotalXP { get; set; }
        public int CurrentStreak { get; set; }
        public SpriteConfigDto CharacterSprite { get; set; }
    }
}
