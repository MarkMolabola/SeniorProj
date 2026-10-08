using MDRNJ.Api.Data;
using MDRNJ.Api.Data.Entities;
using MDRNJ.Api.Services.Interfaces;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Enums;
using MDRNJ.Shared.Game;
using Microsoft.EntityFrameworkCore;

namespace MDRNJ.Api.Services
{
    public class GoalProgressService : IGoalProgressService
    {
        public const string QuestKind = "Quest";

        private readonly AppDbContext _db;

        public GoalProgressService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<GameStatsEntity> GetOrCreateStatsAsync(Guid userId)
        {
            // Local first: a stats row added earlier in this request isn't in the database yet
            var stats = _db.GameStats.Local.FirstOrDefault(g => g.UserId == userId)
                ?? await _db.GameStats.FirstOrDefaultAsync(g => g.UserId == userId);
            if (stats is null)
            {
                stats = new GameStatsEntity { UserId = userId };
                _db.GameStats.Add(stats);
            }
            return stats;
        }

        public void GrantXP(GameStatsEntity stats, int amount, XPReason reason)
        {
            if (amount <= 0) return;

            stats.TotalXP += amount;
            stats.CurrentLevel = AccountProgression.LevelFor(stats.TotalXP);
            _db.XPTransactions.Add(new XPTransactionEntity
            {
                UserId = stats.UserId,
                Amount = amount,
                Reason = reason
            });
        }

        public async Task<List<GoalCompletion>> AdvanceForWorkoutAsync(GameStatsEntity stats, WorkoutSessionEntity session)
        {
            var userId = stats.UserId;
            var when = session.StartTime;
            var completions = new List<GoalCompletion>();

            var quests = await _db.UserQuests
                .Include(uq => uq.Quest)
                .Where(uq => uq.UserId == userId && uq.Status == QuestStatus.Active && uq.Quest.IsActive)
                .ToListAsync();

            foreach (var uq in quests)
            {
                var quest = uq.Quest;
                if (!InWindow(when, quest.StartDate, quest.EndDate)) continue;

                uq.Progress = Advance(uq.Progress, quest.QuestType, session, stats);
                if (uq.Progress < quest.TargetValue) continue;

                uq.Status = QuestStatus.Completed;
                uq.CompletedAt = DateTime.UtcNow;
                completions.Add(CompleteQuest(stats, quest));
            }

            return completions;
        }

        public GoalCompletion CompleteQuest(GameStatsEntity stats, QuestEntity quest)
        {
            GrantXP(stats, quest.XPReward, XPReason.QuestCompleted);
            return new GoalCompletion(
                new CompletedGoalDto { Kind = QuestKind, Id = quest.QuestId, Title = quest.Title, XPReward = quest.XPReward },
                quest.QuestType);
        }

        /// <summary>
        /// How a workout moves each goal type. XP goals count only the
        /// workout's own XP, not rewards from goals it completes.
        /// </summary>
        private static float Advance(float progress, ChallengeType type, WorkoutSessionEntity session, GameStatsEntity stats) => type switch
        {
            ChallengeType.Distance     => progress + session.Exercises.Sum(e => e.DistanceKm ?? 0),
            ChallengeType.WorkoutCount => progress + 1,
            ChallengeType.Streak       => Math.Max(progress, stats.CurrentStreak),
            ChallengeType.XPRace       => progress + session.XPAwarded,
            ChallengeType.FriendDuel   => progress + session.XPAwarded,
            _                          => progress
        };

        /// <summary>Whole days, inclusive: a goal ending today still counts today's workouts.</summary>
        private static bool InWindow(DateTime when, DateTime? start, DateTime? end) =>
            (start is null || when.Date >= start.Value.Date)
            && (end is null || when.Date <= end.Value.Date);
    }
}
