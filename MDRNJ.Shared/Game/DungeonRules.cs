namespace MDRNJ.Shared.Game
{
    /// <summary>A dungeon monster. Art lives in the app (DungeonSprites).</summary>
    public sealed record DungeonMonster(string Id, string Name, float MinPower);

    /// <summary>
    /// One dungeon floor. Clearing it takes <see cref="KillsToClear"/> kills, and going
    /// below needs the next floor's <see cref="MinPower"/>. Deeper floors pay more gold.
    /// </summary>
    /// <param name="MonsterId">Sprite id (DungeonSprites); deeper floors reuse art under tougher names.</param>
    public sealed record DungeonFloor(int Number, string MonsterId, string MonsterName, float MinPower, int KillsToClear, float GoldMultiplier);

    /// <summary>A weekly boss: worn down by dungeon kills, once the hero has the power to fight it.</summary>
    public sealed record DungeonBoss(string Id, string MonsterId, string Name, float MinPower, int Hp);

    /// <summary>
    /// Passive dungeon: the hero slays monsters while you're away and banks gold.
    /// Power comes from STR + END + FLX (trained points plus gear or hero outfit boosts).
    /// Gold fills at <see cref="GoldPerHourPerPower"/> × power × the floor's multiplier
    /// per hour, up to <see cref="CapHours"/> hours' worth, until the player taps COLLECT.
    /// Kills build up the same way and clear floors, damage the weekly boss, and can
    /// drop dungeon-only Legendary gear. Shared so the API (payouts) and the app (live
    /// counter, animation) agree.
    /// </summary>
    public static class DungeonRules
    {
        public const float GoldPerHourPerPower = 2f;
        public const int CapHours = 8;

        /// <summary>Kills per hour for each point of power.</summary>
        public const float KillsPerHourPerPower = 1f;

        public static readonly IReadOnlyList<DungeonFloor> Floors = new[]
        {
            new DungeonFloor(1, "slime",    "Slime",           0,  20,  1.00f),
            new DungeonFloor(2, "bat",      "Cave Bat",        6,  60,  1.15f),
            new DungeonFloor(3, "skeleton", "Skeleton",        10, 120, 1.30f),
            new DungeonFloor(4, "demon",    "Demon",           16, 200, 1.45f),
            new DungeonFloor(5, "slime",    "Giant Slime",     20, 300, 1.60f),
            new DungeonFloor(6, "bat",      "Vampire Bat",     25, 420, 1.75f),
            new DungeonFloor(7, "skeleton", "Skeleton Knight", 30, 560, 1.90f),
            new DungeonFloor(8, "demon",    "Demon Lord",      36, 0,   2.00f), // the bottom: nothing left to clear
        };

        /// <summary>One boss a week (Monday 00:00 UTC), in this order.</summary>
        public static readonly IReadOnlyList<DungeonBoss> Bosses = new[]
        {
            new DungeonBoss("slime_king",     "slime",    "Slime King",     6,  150),
            new DungeonBoss("bat_queen",      "bat",      "Bat Queen",      8,  200),
            new DungeonBoss("bone_lord",      "skeleton", "Bone Lord",      10, 260),
            new DungeonBoss("demon_overlord", "demon",    "Demon Overlord", 14, 320),
        };

        public const int BossGold = 400;

        /// <summary>Extra gold from the boss when there's no dungeon-only gear left to drop.</summary>
        public const int BossGoldNoDrop = 600;

        /// <summary>Chance per kill, times the floor number, that a collect drops dungeon-only gear.</summary>
        public const double DropChancePerKillPerFloor = 0.0002;
        public const double MaxDropChance = 0.10;

        /// <summary>The four monster sprites, weakest first (the Home animation mixes in weaker ones).</summary>
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

        public static DungeonFloor Floor(int number) => Floors[Math.Clamp(number, 1, Floors.Count) - 1];

        public static bool IsBottom(int floor) => floor >= Floors.Count;

        public static float GoldPerHour(float power, int floor = 1) => power * GoldPerHourPerPower * Floor(floor).GoldMultiplier;

        public static int CapGold(float power, int floor = 1) => (int)MathF.Floor(GoldPerHour(power, floor) * CapHours);

        /// <summary>Gold banked after <paramref name="elapsed"/> (capped).</summary>
        public static int PendingGold(float power, TimeSpan elapsed, int floor = 1) =>
            (int)Math.Floor(CappedHours(elapsed) * GoldPerHour(power, floor));

        public static float KillsPerHour(float power) => power * KillsPerHourPerPower;

        /// <summary>Monsters slain after <paramref name="elapsed"/> (capped like gold).</summary>
        public static int PendingKills(float power, TimeSpan elapsed) =>
            (int)Math.Floor(CappedHours(elapsed) * KillsPerHour(power));

        public static double DropChance(int kills, int floor) =>
            Math.Min(MaxDropChance, kills * DropChancePerKillPerFloor * Math.Max(1, floor));

        private static double CappedHours(TimeSpan elapsed) => Math.Clamp(elapsed.TotalHours, 0, CapHours);

        // ── Weekly boss ────────────────────────────────────────────────────

        /// <summary>Monday 00:00 UTC of the week containing <paramref name="utc"/>.</summary>
        public static DateTime WeekStart(DateTime utc)
        {
            var daysSinceMonday = ((int)utc.DayOfWeek + 6) % 7;
            return utc.Date.AddDays(-daysSinceMonday);
        }

        /// <summary>This week's boss; the rotation is the same for every player.</summary>
        public static DungeonBoss BossFor(DateTime weekStart)
        {
            var weeks = (int)((weekStart - new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc)).TotalDays / 7); // a Monday
            return Bosses[((weeks % Bosses.Count) + Bosses.Count) % Bosses.Count];
        }
    }
}
