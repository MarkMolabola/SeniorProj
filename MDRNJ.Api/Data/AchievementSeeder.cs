using MDRNJ.Api.Data.Entities;
using MDRNJ.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MDRNJ.Api.Data
{
    public static class AchievementSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Achievements.AnyAsync()) return;

            var achievements = new List<AchievementEntity>
            {
                // Workout milestones
                new() { Name = "First Sweat",      Description = "Complete your first workout.",               BadgeImageUrl = "badges/first_sweat.png",      XPReward = 50,   Rarity = AchievementRarity.Common },
                new() { Name = "10 Workouts",       Description = "Complete 10 workouts.",                      BadgeImageUrl = "badges/10_workouts.png",       XPReward = 100,  Rarity = AchievementRarity.Common },
                new() { Name = "50 Workouts",       Description = "Complete 50 workouts.",                      BadgeImageUrl = "badges/50_workouts.png",       XPReward = 300,  Rarity = AchievementRarity.Uncommon },
                new() { Name = "Century Club",      Description = "Complete 100 workouts.",                     BadgeImageUrl = "badges/100_workouts.png",      XPReward = 750,  Rarity = AchievementRarity.Rare },

                // Streaks
                new() { Name = "3-Day Streak",      Description = "Work out 3 days in a row.",                  BadgeImageUrl = "badges/streak_3.png",         XPReward = 75,   Rarity = AchievementRarity.Common },
                new() { Name = "7-Day Streak",      Description = "Work out 7 days in a row.",                  BadgeImageUrl = "badges/streak_7.png",         XPReward = 200,  Rarity = AchievementRarity.Uncommon },
                new() { Name = "30-Day Streak",     Description = "Work out 30 days in a row.",                 BadgeImageUrl = "badges/streak_30.png",        XPReward = 1000, Rarity = AchievementRarity.Rare },
                new() { Name = "Unstoppable",       Description = "Work out 100 days in a row.",                BadgeImageUrl = "badges/streak_100.png",       XPReward = 5000, Rarity = AchievementRarity.Legendary },

                // XP milestones
                new() { Name = "Level 5",           Description = "Reach level 5.",                             BadgeImageUrl = "badges/level_5.png",          XPReward = 100,  Rarity = AchievementRarity.Common },
                new() { Name = "Level 10",          Description = "Reach level 10.",                            BadgeImageUrl = "badges/level_10.png",         XPReward = 250,  Rarity = AchievementRarity.Uncommon },
                new() { Name = "Level 25",          Description = "Reach level 25.",                            BadgeImageUrl = "badges/level_25.png",         XPReward = 750,  Rarity = AchievementRarity.Rare },
                new() { Name = "Level 50",          Description = "Reach level 50.",                            BadgeImageUrl = "badges/level_50.png",         XPReward = 2500, Rarity = AchievementRarity.Epic },
                new() { Name = "Legend",            Description = "Reach level 100.",                           BadgeImageUrl = "badges/level_100.png",        XPReward = 10000,Rarity = AchievementRarity.Legendary },

                // Social
                new() { Name = "Social Butterfly",  Description = "Add your first friend.",                     BadgeImageUrl = "badges/first_friend.png",     XPReward = 50,   Rarity = AchievementRarity.Common },
                new() { Name = "Squad Goals",       Description = "Have 10 friends.",                           BadgeImageUrl = "badges/squad.png",            XPReward = 200,  Rarity = AchievementRarity.Uncommon },
                new() { Name = "Challenge Accepted", Description = "Complete your first challenge.",            BadgeImageUrl = "badges/challenge_1.png",      XPReward = 100,  Rarity = AchievementRarity.Common },
                new() { Name = "Challenger",        Description = "Win 10 challenges.",                         BadgeImageUrl = "badges/challenge_10.png",     XPReward = 500,  Rarity = AchievementRarity.Rare },

                // Character
                new() { Name = "Transformation",   Description = "Evolve your character for the first time.",  BadgeImageUrl = "badges/evolve_1.png",         XPReward = 150,  Rarity = AchievementRarity.Common },
                new() { Name = "Power Build",       Description = "Reach Muscular body type.",                  BadgeImageUrl = "badges/body_muscular.png",    XPReward = 500,  Rarity = AchievementRarity.Rare },
                new() { Name = "Elite Form",        Description = "Reach Powerlifter body type.",               BadgeImageUrl = "badges/body_powerlifter.png", XPReward = 1500, Rarity = AchievementRarity.Epic },
            };

            db.Achievements.AddRange(achievements);
            await db.SaveChangesAsync();
        }
    }
}
