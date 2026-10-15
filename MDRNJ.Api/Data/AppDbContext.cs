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
        public DbSet<GoldTransactionEntity> GoldTransactions => Set<GoldTransactionEntity>();
        public DbSet<AchievementEntity> Achievements => Set<AchievementEntity>();
        public DbSet<UserAchievementEntity> UserAchievements => Set<UserAchievementEntity>();
        public DbSet<CharacterStateEntity> CharacterStates => Set<CharacterStateEntity>();
        public DbSet<ActivityScoreEntity> ActivityScores => Set<ActivityScoreEntity>();
        public DbSet<FriendshipEntity> Friendships => Set<FriendshipEntity>();
        public DbSet<ChallengeEntity> Challenges => Set<ChallengeEntity>();
        public DbSet<ChallengeAttemptEntity> ChallengeAttempts => Set<ChallengeAttemptEntity>();
        public DbSet<QuestEntity> Quests => Set<QuestEntity>();
        public DbSet<UserQuestEntity> UserQuests => Set<UserQuestEntity>();
        public DbSet<UserGearEntity> UserGear => Set<UserGearEntity>();
        public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
        public DbSet<ChatChannelEntity> ChatChannels => Set<ChatChannelEntity>();
        public DbSet<ChatMemberEntity> ChatMembers => Set<ChatMemberEntity>();
        public DbSet<ChatMessageEntity> ChatMessages => Set<ChatMessageEntity>();
        public DbSet<ChatReportEntity> ChatReports => Set<ChatReportEntity>();
        public DbSet<UserBlockEntity> UserBlocks => Set<UserBlockEntity>();

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
                // Existing accounts get the starting gold when the column is added
                e.Property(g => g.Gold).HasDefaultValue(MDRNJ.Shared.Game.GoldRules.StartingGold);
                // Existing heroes start their first dungeon run when the column is added
                e.Property(g => g.DungeonCollectedAt).HasDefaultValueSql("GETUTCDATE()");
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

            // GoldTransactionEntity
            modelBuilder.Entity<GoldTransactionEntity>(e =>
            {
                e.HasKey(x => x.TransactionId);
                e.Property(x => x.Description).HasMaxLength(200);
                e.HasIndex(x => new { x.UserId, x.CreatedAt });
                e.HasOne(x => x.User)
                 .WithMany()
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
                e.Property(c => c.HeadGear).HasMaxLength(60);
                e.Property(c => c.TopGear).HasMaxLength(60);
                e.Property(c => c.BottomGear).HasMaxLength(60);
                e.Property(c => c.ShoesGear).HasMaxLength(60);
                e.Property(c => c.AccessoryGear).HasMaxLength(60);
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

            // RefreshTokenEntity — looked up by hash on every refresh
            modelBuilder.Entity<RefreshTokenEntity>(e =>
            {
                e.HasKey(t => t.RefreshTokenId);
                e.Property(t => t.TokenHash).HasMaxLength(32).IsRequired();
                e.HasIndex(t => t.TokenHash).IsUnique();
                e.HasOne(t => t.User)
                 .WithMany(u => u.RefreshTokens)
                 .HasForeignKey(t => t.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // UserGearEntity — shop items a user owns
            modelBuilder.Entity<UserGearEntity>(e =>
            {
                e.HasKey(g => new { g.UserId, g.ItemId });
                e.Property(g => g.ItemId).HasMaxLength(60);
                e.HasOne(g => g.User)
                 .WithMany(u => u.OwnedGear)
                 .HasForeignKey(g => g.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ChatChannelEntity — the global room is seeded
            modelBuilder.Entity<ChatChannelEntity>(e =>
            {
                e.HasKey(c => c.ChannelId);
                e.Property(c => c.Name).HasMaxLength(MDRNJ.Shared.Game.ChatRules.MaxGroupNameLength);
                e.Property(c => c.DirectKey).HasMaxLength(80);
                e.HasIndex(c => c.DirectKey).IsUnique();
                e.Property(c => c.LastMessagePreview).HasMaxLength(120);
                e.Property(c => c.LastSenderName).HasMaxLength(30);
                e.HasData(new ChatChannelEntity
                {
                    ChannelId = MDRNJ.Shared.Game.ChatRules.GlobalChannelId,
                    Kind = MDRNJ.Shared.Enums.ChatChannelKind.Global,
                    Name = "Global",
                    CreatedAt = new DateTime(2026, 10, 14, 0, 0, 0, DateTimeKind.Utc)
                });
            });

            // ChatMemberEntity — composite key
            modelBuilder.Entity<ChatMemberEntity>(e =>
            {
                e.HasKey(m => new { m.ChannelId, m.UserId });
                e.HasOne(m => m.Channel)
                 .WithMany(c => c.Members)
                 .HasForeignKey(m => m.ChannelId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(m => m.User)
                 .WithMany()
                 .HasForeignKey(m => m.UserId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(m => m.UserId);
            });

            // ChatMessageEntity — paged newest-first per channel
            modelBuilder.Entity<ChatMessageEntity>(e =>
            {
                e.HasKey(m => m.MessageId);
                e.Property(m => m.Body).HasMaxLength(MDRNJ.Shared.Game.ChatRules.MaxMessageLength).IsRequired();
                e.HasOne(m => m.Channel)
                 .WithMany(c => c.Messages)
                 .HasForeignKey(m => m.ChannelId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(m => m.Sender)
                 .WithMany()
                 .HasForeignKey(m => m.SenderId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasIndex(m => new { m.ChannelId, m.SentAt });
                e.HasIndex(m => new { m.SenderId, m.SentAt });
            });

            // ChatReportEntity — the reporter link is Restrict (a second cascade path
            // from Users via ChatMessages isn't allowed); account deletion removes reports first
            modelBuilder.Entity<ChatReportEntity>(e =>
            {
                e.HasKey(r => r.ReportId);
                e.Property(r => r.Reason).HasMaxLength(40).IsRequired();
                e.HasOne(r => r.Message)
                 .WithMany()
                 .HasForeignKey(r => r.MessageId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(r => r.Reporter)
                 .WithMany()
                 .HasForeignKey(r => r.ReporterId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(r => new { r.MessageId, r.ReporterId }).IsUnique();
            });

            // UserBlockEntity — two links to Users, so both Restrict (like friendships)
            modelBuilder.Entity<UserBlockEntity>(e =>
            {
                e.HasKey(b => new { b.BlockerId, b.BlockedId });
                e.HasOne(b => b.Blocker)
                 .WithMany()
                 .HasForeignKey(b => b.BlockerId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(b => b.Blocked)
                 .WithMany()
                 .HasForeignKey(b => b.BlockedId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(b => b.BlockedId);
            });
        }
    }
}
