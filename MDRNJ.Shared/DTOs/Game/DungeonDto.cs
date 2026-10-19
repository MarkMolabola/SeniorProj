namespace MDRNJ.Shared.DTOs.Game
{
    /// <summary>The hero's passive dungeon run (see MDRNJ.Shared.Game.DungeonRules).</summary>
    public class DungeonDto
    {
        /// <summary>STR + END + FLX levels, gear included.</summary>
        public float Power { get; set; }
        public float GoldPerHour { get; set; }

        /// <summary>Gold waiting to be collected right now.</summary>
        public int PendingGold { get; set; }
        public int CapGold { get; set; }
        public int CapHours { get; set; }

        /// <summary>
        /// Time since the last collect, as the server saw it (capped at CapHours).
        /// The app adds its own elapsed time to keep the counter ticking without clock skew.
        /// </summary>
        public double SecondsAccrued { get; set; }

        public string MonsterId { get; set; } = "slime";
        public string MonsterName { get; set; } = "Slime";

        // ── Floors ──
        public int Floor { get; set; } = 1;
        public int FloorCount { get; set; }
        public float GoldMultiplier { get; set; } = 1;
        public float KillsPerHour { get; set; }

        /// <summary>Kills banked on this floor (not counting the pending run).</summary>
        public int FloorKills { get; set; }

        /// <summary>Kills needed to clear this floor; 0 on the bottom floor.</summary>
        public int KillsToClear { get; set; }

        /// <summary>Power needed for the floor below, when the hero doesn't have it yet.</summary>
        public float? NextFloorMinPower { get; set; }

        // ── Weekly boss ──
        public string BossName { get; set; } = string.Empty;
        public string BossMonsterId { get; set; } = "slime";
        public int BossHp { get; set; }
        public int BossDamage { get; set; }
        public float BossMinPower { get; set; }
        public bool BossDefeated { get; set; }

        /// <summary>When this week's boss leaves (next Monday 00:00 UTC).</summary>
        public DateTime BossEndsAt { get; set; }
    }
}
