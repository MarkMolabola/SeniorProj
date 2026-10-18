using System;
using System.Collections.Generic;

namespace MDRNJ.Api.Data.Entities
{
    public class UserEntity
    {
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? PasswordHash { get; set; }   // null for Google-only accounts
        public string? AvatarUrl { get; set; }
        public DateTime DateJoined { get; set; } = DateTime.UtcNow;
        public bool IsAdmin { get; set; } = false;

        // Which Terms of Service / Privacy Policy version (LegalRules.TermsVersion) the player accepted
        public int TermsVersion { get; set; }
        public DateTime? TermsAcceptedAt { get; set; }

        // Moderation (set by admins)
        public DateTime? ChatMutedUntil { get; set; }
        public bool IsBanned { get; set; }

        // Email verification
        public bool EmailVerified { get; set; } = false;
        public string? VerificationCodeHash { get; set; }
        public DateTime? VerificationCodeExpiresAt { get; set; }
        public DateTime? VerificationCodeSentAt { get; set; }
        public int VerificationAttempts { get; set; }

        // Forgot password (same 6-digit code scheme as verification)
        public string? PasswordResetCodeHash { get; set; }
        public DateTime? PasswordResetExpiresAt { get; set; }
        public DateTime? PasswordResetSentAt { get; set; }
        public int PasswordResetAttempts { get; set; }

        // External login
        public string? GoogleId { get; set; }

        // Navigation
        public UserProfileEntity? Profile { get; set; }
        public GameStatsEntity? GameStats { get; set; }
        public CharacterStateEntity? CharacterState { get; set; }
        public ActivityScoreEntity? ActivityScore { get; set; }
        public ICollection<WorkoutSessionEntity> WorkoutSessions { get; set; } = new List<WorkoutSessionEntity>();
        public ICollection<XPTransactionEntity> XPTransactions { get; set; } = new List<XPTransactionEntity>();
        public ICollection<UserAchievementEntity> UserAchievements { get; set; } = new List<UserAchievementEntity>();
        public ICollection<FriendshipEntity> SentFriendRequests { get; set; } = new List<FriendshipEntity>();
        public ICollection<FriendshipEntity> ReceivedFriendRequests { get; set; } = new List<FriendshipEntity>();
        public ICollection<ChallengeEntity> IssuedChallenges { get; set; } = new List<ChallengeEntity>();
        public ICollection<ChallengeEntity> ReceivedChallenges { get; set; } = new List<ChallengeEntity>();
        public ICollection<UserQuestEntity> UserQuests { get; set; } = new List<UserQuestEntity>();
        public ICollection<UserGearEntity> OwnedGear { get; set; } = new List<UserGearEntity>();
        public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = new List<RefreshTokenEntity>();
    }
}
