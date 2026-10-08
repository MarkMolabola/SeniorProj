namespace MDRNJ.Shared.Game
{
    /// <summary>
    /// Stat points (STR / END / FLX) only ever go up. Every
    /// <see cref="PointsPerLevel"/> points is one stat level. Shared so the API
    /// (level-up detection) and the app (bars) agree on the maths.
    /// </summary>
    public static class StatProgression
    {
        public const float PointsPerLevel = 10f;

        public static int LevelFor(float points) =>
            (int)MathF.Floor(MathF.Max(0, points) / PointsPerLevel) + 1;

        /// <summary>0–1 progress through the current stat level.</summary>
        public static double ProgressInLevel(float points) =>
            MathF.Max(0, points) % PointsPerLevel / PointsPerLevel;
    }
}
