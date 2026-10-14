namespace MDRNJ.Shared.DTOs.Game
{
    public class DungeonCollectDto
    {
        public int Collected { get; set; }

        /// <summary>Gold balance after collecting.</summary>
        public int Gold { get; set; }

        /// <summary>The run, restarted from now.</summary>
        public DungeonDto Dungeon { get; set; } = new();
    }
}
