namespace MDRNJ.Shared.DTOs.Game
{
    public class DungeonCollectDto
    {
        public int Collected { get; set; }

        /// <summary>Gold balance after collecting.</summary>
        public int Gold { get; set; }

        /// <summary>Monsters slain since the last collect.</summary>
        public int Kills { get; set; }

        /// <summary>Set when this collect cleared floors and the hero went deeper.</summary>
        public int? NewFloor { get; set; }

        /// <summary>Damage dealt to the weekly boss by this collect.</summary>
        public int BossDamage { get; set; }

        /// <summary>The weekly boss fell on this collect; <see cref="BossGold"/> is its reward.</summary>
        public bool BossDefeated { get; set; }
        public int BossGold { get; set; }

        /// <summary>Dungeon-only gear that dropped (GearCatalog id), if any.</summary>
        public string? DroppedItemId { get; set; }

        /// <summary>The run, restarted from now.</summary>
        public DungeonDto Dungeon { get; set; } = new();
    }
}
