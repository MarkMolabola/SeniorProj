namespace MDRNJ.Shared.Game
{
    /// <summary>A dungeon monster. Art lives in the app (DungeonSprites).</summary>
    public sealed record DungeonMonster(string Id, string Name, float MinPower);

    /// <summary>
    /// Passive dungeon: the hero slays monsters while you're away and banks gold.
    /// Power comes from STR + END + FLX (trained points plus gear boosts); gold
    /// fills at <see cref="GoldPerHourPerPower"/> × power per hour, up to
    /// <see cref="CapHours"/> hours' worth, until the player taps COLLECT.
    /// Shared so the API (payouts) and the app (live counter, animation) agree.
    /// </summary>
    public static class DungeonRules
    {
        public const float GoldPerHourPerPower = 2f;
        public const int CapHours = 8;

        /// <summary>Tougher monsters appear as the hero gets stronger (lowest first).</summary>
        public static readonly IReadOnlyList<DungeonMonster> Monsters = new[]
        {
            new DungeonMonster("slime", "Slime", 0),
            new DungeonMonster("bat", "Cave Bat", 6),
            new DungeonMonster("skeleton", "Skeleton", 10),
            new DungeonMonster("demon", "Demon", 16),
        };

        /// <summary>
        /// Sum of the three stat levels, fractional: a brand-new hero (all Lv 1) has 3.
        /// Takes effective points (trained + gear).
        /// </summary>
        public static float Power(float strength, float endurance, float flexibility) =>
            3 + (MathF.Max(0, strength) + MathF.Max(0, endurance) + MathF.Max(0, flexibility)) / StatProgression.PointsPerLevel;

        public static float GoldPerHour(float power) => power * GoldPerHourPerPower;

        public static int CapGold(float power) => (int)MathF.Floor(GoldPerHour(power) * CapHours);

        /// <summary>Gold banked after <paramref name="elapsed"/> (capped).</summary>
        public static int PendingGold(float power, TimeSpan elapsed)
        {
            var hours = Math.Clamp(elapsed.TotalHours, 0, CapHours);
            return (int)Math.Floor(hours * GoldPerHour(power));
        }

        /// <summary>The toughest monster this hero can take on.</summary>
        public static DungeonMonster MonsterFor(float power) =>
            Monsters.Last(m => power >= m.MinPower);
    }
}
