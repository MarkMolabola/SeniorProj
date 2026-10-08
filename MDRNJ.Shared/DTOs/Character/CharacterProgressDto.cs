namespace MDRNJ.Shared.DTOs.Character
{
    /// <summary>
    /// What changed on the character after a workout, quest or challenge.
    /// Returned alongside those results so the app can show the reward /
    /// evolution popup.
    /// </summary>
    public class CharacterProgressDto
    {
        public float StrengthGained { get; set; }
        public float CardioGained { get; set; }
        public float FlexibilityGained { get; set; }

        public List<StatLevelUpDto> LevelUps { get; set; } = new();

        /// <summary>True when the outfit or body type changed.</summary>
        public bool Evolved { get; set; }
        public string? PreviousOutfitSlot { get; set; }
        public int? PreviousBodyType { get; set; }

        public CharacterStateDto Character { get; set; } = null!;
        public ActivityScoreDto ActivityScore { get; set; } = null!;
    }

    public class StatLevelUpDto
    {
        /// <summary>"STR", "END" or "FLX".</summary>
        public string Stat { get; set; } = null!;
        public int NewLevel { get; set; }
    }
}
