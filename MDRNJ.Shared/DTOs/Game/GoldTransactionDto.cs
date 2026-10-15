namespace MDRNJ.Shared.DTOs.Game
{
    /// <summary>One line of the gold history: earned (positive) or spent (negative).</summary>
    public class GoldTransactionDto
    {
        public Guid TransactionId { get; set; }
        public int Amount { get; set; }

        /// <summary>A <see cref="MDRNJ.Shared.Enums.GoldReason"/> name.</summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>What exactly, e.g. the quest title or the item bought.</summary>
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
