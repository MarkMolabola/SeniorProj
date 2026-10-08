using System;

namespace MDRNJ.Api.Data.Entities
{
    /// <summary>
    /// A refresh token. Only its SHA-256 hash is stored, so a database leak
    /// doesn't leak usable tokens. Each use replaces it with a new one.
    /// </summary>
    public class RefreshTokenEntity
    {
        public Guid RefreshTokenId { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public byte[] TokenHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }

        /// <summary>Set when used (rotated) or on logout; a revoked token can't be used again.</summary>
        public DateTime? RevokedAt { get; set; }

        // Navigation
        public UserEntity User { get; set; } = null!;
    }
}
