using MDRNJ.Shared.Enums;

namespace MDRNJ.Shared.Game
{
    /// <summary>
    /// Scoring rules for 1v1 friend challenges. Shared so the API (winner)
    /// and the app (labels, input) agree.
    /// </summary>
    public static class DuelRules
    {
        public const int WinXP = 150;
        public const int TieXP = 75;
        public const int LossXP = 25;

        /// <summary>Allowed challenge lengths, in hours.</summary>
        public static readonly int[] DurationHours = { 24, 72, 168 };

        /// <summary>A best-attempt run is a race over a distance: the lowest time wins.</summary>
        public static bool IsTimedRace(DuelContest contest, DuelScoring scoring) =>
            contest == DuelContest.Run && scoring == DuelScoring.BestAttempt;

        public static bool HigherIsBetter(DuelContest contest, DuelScoring scoring) =>
            !IsTimedRace(contest, scoring);

        /// <summary>Attempts recorded with the in-app stopwatch (value = seconds).</summary>
        public static bool UsesStopwatch(DuelContest contest, DuelScoring scoring) =>
            contest == DuelContest.Plank || IsTimedRace(contest, scoring);

        /// <summary>
        /// The submitted value's unit: "reps", "sec" or "km". Timed races
        /// submit seconds too.
        /// </summary>
        public static string Unit(DuelContest contest, DuelScoring scoring) => contest switch
        {
            DuelContest.PushUps or DuelContest.PullUps => "reps",
            DuelContest.Plank => "sec",
            _ => scoring == DuelScoring.Total ? "km" : "sec"
        };

        /// <summary>Folds a new attempt into a score: Total adds up, BestAttempt keeps the better one.</summary>
        public static float Combine(float? current, float attempt, DuelContest contest, DuelScoring scoring)
        {
            if (current is not float score) return attempt;
            if (scoring == DuelScoring.Total) return score + attempt;
            return HigherIsBetter(contest, scoring) ? Math.Max(score, attempt) : Math.Min(score, attempt);
        }

        /// <summary>+1 if <paramref name="a"/> wins, -1 if <paramref name="b"/> wins, 0 for a tie. Missing scores lose.</summary>
        public static int Compare(float? a, float? b, DuelContest contest, DuelScoring scoring)
        {
            if (a is null && b is null) return 0;
            if (b is null) return 1;
            if (a is null) return -1;
            if (a.Value == b.Value) return 0;
            var aHigher = a.Value > b.Value;
            return aHigher == HigherIsBetter(contest, scoring) ? 1 : -1;
        }

        /// <summary>"42 reps", "1:35", "3.2 km".</summary>
        public static string Format(float value, DuelContest contest, DuelScoring scoring)
        {
            var unit = Unit(contest, scoring);
            if (unit == "sec")
            {
                var t = TimeSpan.FromSeconds(Math.Round(value));
                return t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss") : t.ToString(@"m\:ss");
            }
            return unit == "km" ? $"{value:0.##} km" : $"{value:0} reps";
        }

        public static string ContestName(DuelContest contest) => contest switch
        {
            DuelContest.PushUps => "Push-ups",
            DuelContest.PullUps => "Pull-ups",
            DuelContest.Plank => "Plank",
            _ => "Run"
        };
    }
}
