using System;
using System.Collections.Generic;

namespace MDRNJ.Api.Data.Entities
{
    public class UserEntity
    {
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public DateTime DateJoined { get; set; } = DateTime.UtcNow;
        public bool IsAdmin { get; set; } = false;

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
        public ICollection<ChallengeEntity> CreatedChallenges { get; set; } = new List<ChallengeEntity>();
        public ICollection<ChallengeParticipantEntity> ChallengeParticipations { get; set; } = new List<ChallengeParticipantEntity>();
        public ICollection<UserQuestEntity> UserQuests { get; set; } = new List<UserQuestEntity>();
    }
}
