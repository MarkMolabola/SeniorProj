using System;

namespace MDRNJ.Api.Data.Entities
{
    /// <summary>A shop item a user owns (GearCatalog item id).</summary>
    public class UserGearEntity
    {
        public Guid UserId { get; set; }
        public string ItemId { get; set; } = null!;
        public int PricePaid { get; set; }
        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public UserEntity User { get; set; } = null!;
    }
}
