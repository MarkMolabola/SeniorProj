using MDRNJ.Shared.Enums;
using System;

namespace MDRNJ.Api.Services
{
    /// <summary>
    /// Stat points to add to a character. <see cref="Bonus"/> is split across
    /// STR / END / FLX in proportion to the user's recent training, so general
    /// rewards (streaks, workout counts) reinforce how they actually train.
    /// </summary>
    public readonly record struct StatGain(float Strength, float Cardio, float Flexibility, float Bonus = 0)
    {
        public bool IsEmpty => Strength <= 0 && Cardio <= 0 && Flexibility <= 0 && Bonus <= 0;

        public static StatGain operator +(StatGain a, StatGain b) => new(
            a.Strength + b.Strength, a.Cardio + b.Cardio, a.Flexibility + b.Flexibility, a.Bonus + b.Bonus);
    }

    /// <summary>How workouts, quests and friend challenges turn into stat points.</summary>
    public static class StatRules
    {
        /// <summary>Stat points per minute of fully-weighted training (30 min run ≈ +3 END).</summary>
        public const float PointsPerMinute = 0.1f;

        /// <summary>
        /// STR / END / FLX split per workout type (each row sums to 1). Also
        /// used for the training shares that pick body type and outfit.
        /// Keep in sync with the backfill in the AddStatPoints migration.
        /// </summary>
        public static (float Strength, float Cardio, float Flexibility) Weights(WorkoutType type) => type switch
        {
            WorkoutType.Weightlifting => (1.0f, 0.0f, 0.0f),
            WorkoutType.Running       => (0.0f, 1.0f, 0.0f),
            WorkoutType.Cycling       => (0.0f, 1.0f, 0.0f),
            WorkoutType.Swimming      => (0.0f, 1.0f, 0.0f),
            WorkoutType.Rowing        => (0.2f, 0.8f, 0.0f),
            WorkoutType.Yoga          => (0.0f, 0.0f, 1.0f),
            WorkoutType.HIIT          => (0.4f, 0.6f, 0.0f),
            WorkoutType.Boxing        => (0.4f, 0.6f, 0.0f),
            WorkoutType.Climbing      => (0.6f, 0.2f, 0.2f),
            WorkoutType.Tennis        => (0.2f, 0.7f, 0.1f),
            WorkoutType.Basketball    => (0.2f, 0.7f, 0.1f),
            WorkoutType.Football      => (0.2f, 0.7f, 0.1f),
            WorkoutType.Hiking        => (0.2f, 0.8f, 0.0f),
            WorkoutType.Golf          => (0.2f, 0.4f, 0.4f),
            _                         => (0.3f, 0.5f, 0.2f),
        };

        public static StatGain ForWorkout(WorkoutType type, int durationMins)
        {
            var (s, c, f) = Weights(type);
            var points = Math.Max(0, durationMins) * PointsPerMinute;
            return new StatGain(s * points, c * points, f * points);
        }

        /// <summary>
        /// Friend challenge reward: XP earned / 25 points (win 6, tie 3, loss 1),
        /// into the stats the contest trains.
        /// </summary>
        public static StatGain ForDuel(DuelContest contest, int xpEarned)
        {
            var points = xpEarned / 25f;
            return contest switch
            {
                DuelContest.PushUps or DuelContest.PullUps => new StatGain(points, 0, 0),
                DuelContest.Plank => new StatGain(points / 2, points / 2, 0),
                _ => new StatGain(0, points, 0)
            };
        }

        /// <summary>
        /// Quest completion reward: XPReward / 25 points (2–10).
        /// Distance goals are cardio; everything else is a balanced bonus.
        /// </summary>
        public static StatGain ForCompletion(ChallengeType type, int xpReward)
        {
            var points = Math.Clamp(xpReward / 25f, 2f, 10f);
            return type == ChallengeType.Distance
                ? new StatGain(0, points, 0)
                : new StatGain(0, 0, 0, Bonus: points);
        }
    }
}
