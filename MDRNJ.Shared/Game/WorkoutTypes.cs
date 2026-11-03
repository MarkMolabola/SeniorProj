using MDRNJ.Shared.Enums;

namespace MDRNJ.Shared.Game
{
    /// <summary>A picker group: Cardio, Flexibility, Strength or Sports.</summary>
    public sealed record WorkoutCategory(string Name, IReadOnlyList<WorkoutType> Types);

    /// <summary>
    /// The workout types players can pick on the Train tab and Calendar. Admin
    /// quests filter on the same list, so a "3 Running workouts" quest matches
    /// exactly what players log.
    /// </summary>
    public static class WorkoutTypes
    {
        public const string Cardio = "Cardio";
        public const string Flexibility = "Flexibility";
        public const string Strength = "Strength";
        public const string Sports = "Sports";

        /// <summary>Picker order; each category ends with its "Other" catch-all.</summary>
        public static readonly IReadOnlyList<WorkoutCategory> Categories = new[]
        {
            new WorkoutCategory(Cardio, new[]
            {
                WorkoutType.Running, WorkoutType.Jogging, WorkoutType.Walking, WorkoutType.Cycling,
                WorkoutType.Swimming, WorkoutType.Rowing, WorkoutType.Hiking, WorkoutType.HIIT,
                WorkoutType.JumpRope, WorkoutType.Elliptical, WorkoutType.StairClimber, WorkoutType.Dance,
                WorkoutType.OtherCardio
            }),
            new WorkoutCategory(Flexibility, new[]
            {
                WorkoutType.Yoga, WorkoutType.Stretching, WorkoutType.Pilates, WorkoutType.Mobility,
                WorkoutType.TaiChi, WorkoutType.OtherFlexibility
            }),
            new WorkoutCategory(Strength, new[]
            {
                WorkoutType.Weightlifting, WorkoutType.Calisthenics, WorkoutType.Powerlifting,
                WorkoutType.CircuitTraining, WorkoutType.Kettlebell, WorkoutType.Climbing,
                WorkoutType.OtherStrength
            }),
            new WorkoutCategory(Sports, new[]
            {
                WorkoutType.Basketball, WorkoutType.Soccer, WorkoutType.Football, WorkoutType.Tennis,
                WorkoutType.Volleyball, WorkoutType.Baseball, WorkoutType.Badminton, WorkoutType.Pickleball,
                WorkoutType.Golf, WorkoutType.Boxing, WorkoutType.MartialArts, WorkoutType.OtherSport
            }),
        };

        /// <summary>The category a type is listed under; null for the legacy <see cref="WorkoutType.Other"/>.</summary>
        public static WorkoutCategory? CategoryOf(WorkoutType type) =>
            Categories.FirstOrDefault(c => c.Types.Contains(type));

        public static WorkoutCategory? CategoryOf(string? type) =>
            Enum.TryParse<WorkoutType>(type, true, out var parsed) ? CategoryOf(parsed) : null;

        private static readonly Dictionary<WorkoutType, string> DisplayNames = new()
        {
            [WorkoutType.JumpRope] = "Jump Rope",
            [WorkoutType.StairClimber] = "Stair Climber",
            [WorkoutType.OtherCardio] = "Other Cardio",
            [WorkoutType.TaiChi] = "Tai Chi",
            [WorkoutType.OtherFlexibility] = "Other Flexibility",
            [WorkoutType.CircuitTraining] = "Circuit Training",
            [WorkoutType.OtherStrength] = "Other Strength",
            [WorkoutType.MartialArts] = "Martial Arts",
            [WorkoutType.OtherSport] = "Other Sport",
        };

        /// <summary>How a type reads in the UI ("Jump Rope", not "JumpRope").</summary>
        public static string DisplayName(WorkoutType type) =>
            DisplayNames.TryGetValue(type, out var name) ? name : type.ToString();

        /// <summary>Display name for an API type name; unknown names come back as-is.</summary>
        public static string DisplayName(string? type) =>
            Enum.TryParse<WorkoutType>(type, true, out var parsed) && Enum.IsDefined(parsed)
                ? DisplayName(parsed)
                : type ?? string.Empty;

        /// <summary>Quest filter value meaning "any workout counts" (stored as null).</summary>
        public const string Any = "Any";

        /// <summary>
        /// Longest workout that earns rewards. A live session left running is capped
        /// here when it ends, and backlogged workouts can't claim more.
        /// </summary>
        public const int MaxDurationMins = 240;

        public static readonly IReadOnlyList<WorkoutType> Selectable =
            Categories.SelectMany(c => c.Types).ToList();

        public static IReadOnlyList<string> SelectableNames { get; } = Selectable.Select(t => t.ToString()).ToList();

        private static readonly HashSet<WorkoutType> DistanceTypes = new()
        {
            WorkoutType.Running, WorkoutType.Jogging, WorkoutType.Walking, WorkoutType.Cycling,
            WorkoutType.Swimming, WorkoutType.Hiking, WorkoutType.Rowing
        };

        /// <summary>Workouts that cover a distance; logging one advances Distance quests.</summary>
        public static bool TracksDistance(WorkoutType type) => DistanceTypes.Contains(type);

        public static bool TracksDistance(string? type) =>
            Enum.TryParse<WorkoutType>(type, true, out var parsed) && TracksDistance(parsed);

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
