using MDRNJ.Api.Data.Entities;
using MDRNJ.Shared.DTOs.Game;
using MDRNJ.Shared.Enums;

namespace MDRNJ.Api.Services.Interfaces
{
    /// <summary>A quest that just completed, with its type for the stat reward.</summary>
    public record GoalCompletion(CompletedGoalDto Goal, ChallengeType Type);

    /// <summary>
    /// Account XP / level and automatic quest progress. Methods
    /// only change tracked entities; callers save, so a workout and everything
    /// it triggers land in one SaveChangesAsync.
    /// </summary>
    public interface IGoalProgressService
    {
        Task<GameStatsEntity> GetOrCreateStatsAsync(Guid userId);

        /// <summary>
        /// Adds XP, updates the account level and records the transaction.
        /// Each level gained also pays <see cref="MDRNJ.Shared.Game.GoldRules.ForLevelUp"/> gold
        /// and <see cref="MDRNJ.Shared.Game.AccountProgression.StatPointsFor"/> free stat points.
        /// </summary>
        void GrantXP(GameStatsEntity stats, int amount, XPReason reason);

        /// <summary>Adds gold and records it in the gold history.</summary>
        void GrantGold(GameStatsEntity stats, int amount, GoldReason reason, string? description = null);

        /// <summary>Takes gold (caller checks the balance) and records it in the gold history.</summary>
        void SpendGold(GameStatsEntity stats, int amount, GoldReason reason, string? description = null);

        /// <summary>
        /// Advances the user's active quests for a finished workout.
        /// Completed quests grant their XP reward. Call after the
        /// workout's own XP and streak are applied to <paramref name="stats"/>.
        /// </summary>
        Task<List<GoalCompletion>> AdvanceForWorkoutAsync(GameStatsEntity stats, WorkoutSessionEntity session);

        /// <summary>Grants a just-completed quest's XP and gold reward.</summary>
        GoalCompletion CompleteQuest(GameStatsEntity stats, QuestEntity quest);
    }
}
