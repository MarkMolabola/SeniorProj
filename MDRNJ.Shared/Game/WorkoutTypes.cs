using MDRNJ.Shared.Enums;

namespace MDRNJ.Shared.Game
{
    /// <summary>
    /// The workout types players can pick on the Train tab and Calendar. Admin
    /// quests filter on the same list, so a "3 Running workouts" quest matches
    /// exactly what players log.
    /// </summary>
    public static class WorkoutTypes
    {
        /// <summary>Quest filter value meaning "any workout counts" (stored as null).</summary>
        public const string Any = "Any";

        public static readonly IReadOnlyList<WorkoutType> Selectable = new[]
        {
            WorkoutType.Running, WorkoutType.Weightlifting, WorkoutType.Cycling, WorkoutType.Swimming,
            WorkoutType.HIIT, WorkoutType.Yoga, WorkoutType.Boxing, WorkoutType.Hiking,
            WorkoutType.Rowing, WorkoutType.Basketball, WorkoutType.Other
        };

        public static IReadOnlyList<string> SelectableNames { get; } = Selectable.Select(t => t.ToString()).ToList();

        /// <summary>
        /// Parses a quest's workout filter: null / "" / "Any" means any workout.
        /// Returns false for an unknown name.
        /// </summary>
        public static bool TryParseQuestFilter(string? value, out WorkoutType? type)
        {
            type = null;
            if (string.IsNullOrWhiteSpace(value) || value.Equals(Any, StringComparison.OrdinalIgnoreCase))
                return true;
            if (!Enum.TryParse<WorkoutType>(value, true, out var parsed) || !Enum.IsDefined(parsed))
                return false;
            type = parsed;
            return true;
        }
    }
}
