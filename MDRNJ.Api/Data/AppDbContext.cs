using MDRNJ.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MDRNJ.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserEntity> Users => Set<UserEntity>();
        public DbSet<UserProfileEntity> UserProfiles => Set<UserProfileEntity>();
        public DbSet<GameStatsEntity> GameStats => Set<GameStatsEntity>();
        public DbSet<WorkoutSessionEntity> WorkoutSessions => Set<WorkoutSessionEntity>();
        public DbSet<ExerciseEntity> Exercises => Set<ExerciseEntity>();
        public DbSet<XPTransactionEntity> XPTransactions => Set<XPTransactionEntity>();
        public DbSet<AchievementEntity> Achievements => Set<AchievementEntity>();
        public DbSet<UserAchievementEntity> UserAchievements => Set<UserAchievementEntity>();
        public DbSet<CharacterStateEntity> CharacterStates => Set<CharacterStateEntity>();
        public DbSet<ActivityScoreEntity> ActivityScores => Set<ActivityScoreEntity>();
        public DbSet<FriendshipEntity> Friendships => Set<FriendshipEntity>();
        public DbSet<ChallengeEntity> Challenges => Set<ChallengeEntity>();
        public DbSet<ChallengeAttemptEntity> ChallengeAttempts => Set<ChallengeAttemptEntity>();
        public DbSet<QuestEntity> Quests => Set<QuestEntity>();
        public DbSet<UserQuestEntity> UserQuests => Set<UserQuestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // UserEntity
            modelBuilder.Entity<UserEntity>(e =>
            {
                e.HasKey(u => u.UserId);
                e.HasIndex(u => u.Email).IsUnique();
                e.HasIndex(u => u.Username).IsUnique();
                e.Property(u => u.Email).HasMaxLength(256).IsRequired();
                e.Property(u => u.Username).HasMaxLength(30).IsRequired();
                e.HasIndex(u => u.GoogleId).IsUnique();
                e.Property(u => u.GoogleId).HasMaxLength(64);
                e.Property(u => u.VerificationCodeHash).HasMaxLength(64);
            });

            // UserProfileEntity — 1:1 with User
            modelBuilder.Entity<UserProfileEntity>(e =>
            {
                e.HasKey(p => p.ProfileId);
                e.HasOne(p => p.User)
                 .WithOne(u => u.Profile)
                 .HasForeignKey<UserProfileEntity>(p => p.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Property(p => p.DisplayName).HasMaxLength(50).IsRequired();
                e.Property(p => p.Bio).HasMaxLength(300);
            });

            // GameStatsEntity — 1:1 with User
            modelBuilder.Entity<GameStatsEntity>(e =>
            {
                e.HasKey(g => g.StatsId);
                e.HasOne(g => g.User)
                 .WithOne(u => u.GameStats)
                 .HasForeignKey<GameStatsEntity>(g => g.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // WorkoutSessionEntity
            modelBuilder.Entity<WorkoutSessionEntity>(e =>
            {
                e.HasKey(w => w.SessionId);
                e.HasOne(w => w.User)
                 .WithMany(u => u.WorkoutSessions)
                 .HasForeignKey(w => w.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ExerciseEntity
            modelBuilder.Entity<ExerciseEntity>(e =>
            {
                e.HasKey(ex => ex.ExerciseId);
                e.HasOne(ex => ex.Session)
                 .WithMany(w => w.Exercises)
                 .HasForeignKey(ex => ex.SessionId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Property(ex => ex.Name).HasMaxLength(100).IsRequired();
                e.Property(ex => ex.Category).HasMaxLength(50).IsRequired();
            });

            // XPTransactionEntity
            modelBuilder.Entity<XPTransactionEntity>(e =>
            {
                e.HasKey(x => x.TransactionId);
                e.HasOne(x => x.User)
                 .WithMany(u => u.XPTransactions)
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // AchievementEntity
            modelBuilder.Entity<AchievementEntity>(e =>
            {
                e.HasKey(a => a.AchievementId);
                e.Property(a => a.Name).HasMaxLength(100).IsRequired();
                e.Property(a => a.BadgeImageUrl).IsRequired();
            });

            // UserAchievementEntity — composite key
            modelBuilder.Entity<UserAchievementEntity>(e =>
            {
                e.HasKey(ua => new { ua.UserId, ua.AchievementId });
                e.HasOne(ua => ua.User)
                 .WithMany(u => u.UserAchievements)
                 .HasForeignKey(ua => ua.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(ua => ua.Achievement)
                 .WithMany(a => a.UserAchievements)
                 .HasForeignKey(ua => ua.AchievementId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // CharacterStateEntity — 1:1 with User
            modelBuilder.Entity<CharacterStateEntity>(e =>
            {
                e.HasKey(c => c.CharacterId);
                e.HasOne(c => c.User)
                 .WithOne(u => u.CharacterState)
                 .HasForeignKey<CharacterStateEntity>(c => c.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.Property(c => c.AvatarId).HasMaxLength(40).HasDefaultValue("classic");
            });

            // ActivityScoreEntity — 1:1 with User
            modelBuilder.Entity<ActivityScoreEntity>(e =>
            {
                e.HasKey(a => a.ScoreId);
                e.HasOne(a => a.User)
                 .WithOne(u => u.ActivityScore)
                 .HasForeignKey<ActivityScoreEntity>(a => a.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // FriendshipEntity
            modelBuilder.Entity<FriendshipEntity>(e =>
            {
                e.HasKey(f => f.FriendshipId);
                e.HasOne(f => f.Requester)
                 .WithMany(u => u.SentFriendRequests)
                 .HasForeignKey(f => f.RequesterId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(f => f.Addressee)
                 .WithMany(u => u.ReceivedFriendRequests)
                 .HasForeignKey(f => f.AddresseeId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(f => new { f.RequesterId, f.AddresseeId }).IsUnique();
            });

            // ChallengeEntity — 1v1 friend challenge
            modelBuilder.Entity<ChallengeEntity>(e =>
            {
                e.HasKey(c => c.ChallengeId);
                e.HasOne(c => c.Challenger)
                 .WithMany(u => u.IssuedChallenges)
                 .HasForeignKey(c => c.ChallengerId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(c => c.Opponent)
                 .WithMany(u => u.ReceivedChallenges)
                 .HasForeignKey(c => c.OpponentId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(c => new { c.ChallengerId, c.Status });
                e.HasIndex(c => new { c.OpponentId, c.Status });
            });

            // ChallengeAttemptEntity
            modelBuilder.Entity<ChallengeAttemptEntity>(e =>
            {
                e.HasKey(a => a.AttemptId);
                e.HasOne(a => a.Challenge)
                 .WithMany(c => c.Attempts)
                 .HasForeignKey(a => a.ChallengeId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(a => new { a.ChallengeId, a.UserId });
            });

            // QuestEntity
            modelBuilder.Entity<QuestEntity>(e =>
            {
                e.HasKey(q => q.QuestId);
                e.Property(q => q.Title).HasMaxLength(100).IsRequired();
                e.Property(q => q.Description).HasMaxLength(500);
            });

            // UserQuestEntity — composite key
            modelBuilder.Entity<UserQuestEntity>(e =>
            {
                e.HasKey(uq => new { uq.UserId, uq.QuestId });
                e.HasOne(uq => uq.User)
                 .WithMany(u => u.UserQuests)
                 .HasForeignKey(uq => uq.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(uq => uq.Quest)
                 .WithMany(q => q.UserQuests)
                 .HasForeignKey(uq => uq.QuestId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
