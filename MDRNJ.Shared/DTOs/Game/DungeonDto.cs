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
    }
}
