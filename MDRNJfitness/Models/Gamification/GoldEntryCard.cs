using MDRNJ.Shared.DTOs.Game;

namespace MDRNJfitness.Models.Gamification
{
    /// <summary>One gold history line, ready to display.</summary>
    public class GoldEntryCard
    {
        public GoldEntryCard(GoldTransactionDto dto) => Dto = dto;

        public GoldTransactionDto Dto { get; }

        public bool IsSpent => Dto.Amount < 0;

        public string AmountText => IsSpent ? $"−{-Dto.Amount}" : $"+{Dto.Amount}";

        public string ReasonText => Dto.Reason switch
        {
            "Workout" => "WORKOUT",
            "Quest" => "QUEST",
            "Achievement" => "ACHIEVEMENT",
            "Duel" => "DUEL",
            "LevelUp" => "LEVEL UP",
            "Dungeon" => "DUNGEON",
            "Boss" => "BOSS",
            "Purchase" => "SHOP",
            _ => Dto.Reason.ToUpperInvariant()
        };

        public string Detail => Dto.Description ?? string.Empty;

        public string WhenText => DateTime.SpecifyKind(Dto.CreatedAt, DateTimeKind.Utc).ToLocalTime().ToString("MMM d · h:mm tt");
    }
}
