namespace MDRNJ.Shared.DTOs.Character
{
    /// <summary>Puts free stat points (earned by levelling up) into one stat.</summary>
    public class SpendStatPointsDto
    {
        /// <summary>"STR", "END" or "FLX".</summary>
        public string Stat { get; set; } = string.Empty;
        public int Points { get; set; } = 1;
    }
}
