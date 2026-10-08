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

        /// <summary>Adds XP, updates the account level and records the transaction.</summary>
        void GrantXP(GameStatsEntity stats, int amount, XPReason reason);

        /// <summary>
        /// Advances the user's active quests for a finished workout.
        /// Completed quests grant their XP reward. Call after the
        /// workout's own XP and streak are applied to <paramref name="stats"/>.
        /// </summary>
        Task<List<GoalCompletion>> AdvanceForWorkoutAsync(GameStatsEntity stats, WorkoutSessionEntity session);

        /// <summary>Grants a just-completed quest's XP reward.</summary>
        GoalCompletion CompleteQuest(GameStatsEntity stats, QuestEntity quest);
    }
}
