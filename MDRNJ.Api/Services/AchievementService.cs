using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MDRNJ.Api.Services
{
    public class AchievementService : IAchievementService
    {
        private readonly AppDbContext _db;
        private readonly IGoalProgressService _goals;

        public AchievementService(AppDbContext db, IGoalProgressService goals)
        {
            _db = db;
            _goals = goals;
        }

        public async Task<List<AchievementDto>> GetAllAsync(Guid userId)
        {
            var all = await _db.Achievements.ToListAsync();
            var unlocked = await _db.UserAchievements
                .Where(ua => ua.UserId == userId)
                .ToDictionaryAsync(ua => ua.AchievementId, ua => ua.UnlockedAt);

            return all.Select(a => new AchievementDto
            {
                AchievementId = a.AchievementId,
                Name = a.Name,
                Description = a.Description,
                BadgeImageUrl = a.BadgeImageUrl,
                XPReward = a.XPReward,
                Rarity = a.Rarity.ToString(),
                UnlockedAt = unlocked.TryGetValue(a.AchievementId, out var dt) ? dt : null
            }).ToList();
        }

        public async Task<List<AchievementDto>> GetUnlockedAsync(Guid userId)
        {
            var userAchievements = await _db.UserAchievements
                .Include(ua => ua.Achievement)
                .Where(ua => ua.UserId == userId)
                .ToListAsync();

            return userAchievements.Select(ua => new AchievementDto
            {
                AchievementId = ua.AchievementId,
                Name = ua.Achievement.Name,
                Description = ua.Achievement.Description,
                BadgeImageUrl = ua.Achievement.BadgeImageUrl,
                XPReward = ua.Achievement.XPReward,
                Rarity = ua.Achievement.Rarity.ToString(),
                UnlockedAt = ua.UnlockedAt
            }).ToList();
        }

        public async Task<List<AchievementDto>> CheckAndUnlockAsync(Guid userId)
        {
            var stats = await _db.GameStats.FirstOrDefaultAsync(g => g.UserId == userId);
            if (stats is null) return new List<AchievementDto>();

            var character = await _db.CharacterStates.FirstOrDefaultAsync(c => c.UserId == userId);
            var strengthLevel = StatProgression.LevelFor(
                (await _db.ActivityScores.FirstOrDefaultAsync(a => a.UserId == userId))?.StrengthPoints ?? 0);
            var friendCount = await _db.Friendships.CountAsync(f =>
                (f.RequesterId == userId || f.AddresseeId == userId) &&
                f.Status == FriendshipStatus.Accepted);
            var duelsCompleted = await _db.Challenges.CountAsync(c =>
                c.Status == ChallengeStatus.Completed &&
                (c.ChallengerId == userId || c.OpponentId == userId));
            var duelsWon = await _db.Challenges.CountAsync(c => c.WinnerId == userId);

            var unlockedIds = await _db.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AchievementId)
                .ToHashSetAsync();

            var allAchievements = await _db.Achievements.ToListAsync();
            var newlyUnlocked = new List<AchievementDto>();

            // Rewards add XP, which can reach a "Level N" achievement, so check until nothing new unlocks
            bool unlockedThisPass;
            do
            {
                unlockedThisPass = false;
                foreach (var achievement in allAchievements)
                {
                    if (unlockedIds.Contains(achievement.AchievementId)) continue;
                    if (!IsEarned(achievement.Name, stats, character, strengthLevel, friendCount, duelsCompleted, duelsWon)) continue;

                    _db.UserAchievements.Add(new UserAchievementEntity
                    {
                        UserId = userId,
                        AchievementId = achievement.AchievementId
                    });
                    unlockedIds.Add(achievement.AchievementId);
                    unlockedThisPass = true;

                    _goals.GrantXP(stats, achievement.XPReward, XPReason.AchievementUnlocked);
                    _goals.GrantGold(stats, GoldRules.ForAchievement(achievement.XPReward));

                    newlyUnlocked.Add(new AchievementDto
                    {
                        AchievementId = achievement.AchievementId,
                        Name = achievement.Name,
                        Description = achievement.Description,
                        BadgeImageUrl = achievement.BadgeImageUrl,
                        XPReward = achievement.XPReward,
                        Rarity = achievement.Rarity.ToString(),
                        UnlockedAt = DateTime.UtcNow
                    });
                }
            } while (unlockedThisPass);

            if (newlyUnlocked.Count > 0)
                await _db.SaveChangesAsync();

            return newlyUnlocked;
        }

        /// <summary>STR level needed for the build achievements, so one heavy session doesn't unlock them.</summary>
        private const int PowerBuildStrengthLevel = 5;
        private const int EliteFormStrengthLevel = 10;

        private static bool IsEarned(string name, GameStatsEntity stats, CharacterStateEntity? character,
            int strengthLevel, int friendCount, int duelsCompleted, int duelsWon) => name switch
        {
            "First Sweat"        => stats.TotalWorkouts >= 1,
            "10 Workouts"        => stats.TotalWorkouts >= 10,
            "50 Workouts"        => stats.TotalWorkouts >= 50,
            "Century Club"       => stats.TotalWorkouts >= 100,
            "3-Day Streak"       => stats.LongestStreak >= 3,
            "7-Day Streak"       => stats.LongestStreak >= 7,
            "30-Day Streak"      => stats.LongestStreak >= 30,
            "Unstoppable"        => stats.LongestStreak >= 100,
            "Level 5"            => stats.CurrentLevel >= 5,
            "Level 10"           => stats.CurrentLevel >= 10,
            "Level 25"           => stats.CurrentLevel >= 25,
            "Level 50"           => stats.CurrentLevel >= 50,
            "Legend"             => stats.CurrentLevel >= 100,
            "Social Butterfly"   => friendCount >= 1,
            "Squad Goals"        => friendCount >= 10,
            "Challenge Accepted" => duelsCompleted >= 1,
            "Challenger"         => duelsWon >= 10,
            // A hero starts in the default outfit with an Athletic build; leaving either means it evolved
            "Transformation"     => character is not null &&
                                    (character.OutfitSlot != "default" || character.BodyType != BodyType.Athletic),
            "Power Build"        => character?.BodyType is BodyType.Muscular or BodyType.Powerlifter
                                    && strengthLevel >= PowerBuildStrengthLevel,
            "Elite Form"         => character?.BodyType == BodyType.Powerlifter
                                    && strengthLevel >= EliteFormStrengthLevel,
            _                    => false
        };
    }
}
