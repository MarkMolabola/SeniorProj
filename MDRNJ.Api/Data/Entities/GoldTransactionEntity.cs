using MDRNJ.Shared.Enums;
using System;

namespace MDRNJ.Api.Data.Entities
{
    /// <summary>Gold earned (positive) or spent (negative).</summary>
    public class GoldTransactionEntity
    {
        public Guid TransactionId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public int Amount { get; set; }
        public GoldReason Reason { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public UserEntity User { get; set; } = null!;
    }
}
