using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MDRNJ.Api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    AchievementId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    BadgeImageUrl = table.Column<string>(type: "TEXT", nullable: false),
                    XPReward = table.Column<int>(type: "INTEGER", nullable: false),
                    Rarity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.AchievementId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    AvatarUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DateJoined = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "ActivityScores",
                columns: table => new
                {
                    ScoreId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StrengthScore = table.Column<float>(type: "REAL", nullable: false),
                    CardioScore = table.Column<float>(type: "REAL", nullable: false),
                    FlexibilityScore = table.Column<float>(type: "REAL", nullable: false),
                    DominantSport = table.Column<int>(type: "INTEGER", nullable: false),
                    LastCalculated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityScores", x => x.ScoreId);
                    table.ForeignKey(
                        name: "FK_ActivityScores_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Challenges",
                columns: table => new
                {
                    ChallengeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ChallengeType = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetValue = table.Column<float>(type: "REAL", nullable: false),
                    XPReward = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Challenges", x => x.ChallengeId);
                    table.ForeignKey(
                        name: "FK_Challenges_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CharacterStates",
                columns: table => new
                {
                    CharacterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    BodyType = table.Column<int>(type: "INTEGER", nullable: false),
                    OutfitSlot = table.Column<string>(type: "TEXT", nullable: false),
                    EquipmentSlot = table.Column<string>(type: "TEXT", nullable: false),
                    AccessorySlot = table.Column<string>(type: "TEXT", nullable: true),
                    LastEvolved = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterStates", x => x.CharacterId);
                    table.ForeignKey(
                        name: "FK_CharacterStates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Friendships",
                columns: table => new
                {
                    FriendshipId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequesterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AddresseeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendships", x => x.FriendshipId);
                    table.ForeignKey(
                        name: "FK_Friendships_Users_AddresseeId",
                        column: x => x.AddresseeId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Friendships_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameStats",
                columns: table => new
                {
                    StatsId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TotalXP = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentStreak = table.Column<int>(type: "INTEGER", nullable: false),
                    LongestStreak = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalWorkouts = table.Column<int>(type: "INTEGER", nullable: false),
                    LastWorkoutDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameStats", x => x.StatsId);
                    table.ForeignKey(
                        name: "FK_GameStats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AchievementId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => new { x.UserId, x.AchievementId });
                    table.ForeignKey(
                        name: "FK_UserAchievements_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "AchievementId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAchievements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    ProfileId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Bio = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    WeightKg = table.Column<float>(type: "REAL", nullable: true),
                    HeightCm = table.Column<float>(type: "REAL", nullable: true),
                    FitnessGoal = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.ProfileId);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutSessions",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkoutType = table.Column<int>(type: "INTEGER", nullable: false),
                    DurationMins = table.Column<int>(type: "INTEGER", nullable: false),
                    CaloriesBurned = table.Column<float>(type: "REAL", nullable: true),
                    XPAwarded = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_WorkoutSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "XPTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Amount = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XPTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_XPTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChallengeParticipants",
                columns: table => new
                {
                    ChallengeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Progress = table.Column<float>(type: "REAL", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeParticipants", x => new { x.ChallengeId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ChallengeParticipants_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "ChallengeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChallengeParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Sets = table.Column<int>(type: "INTEGER", nullable: true),
                    Reps = table.Column<int>(type: "INTEGER", nullable: true),
                    WeightKg = table.Column<float>(type: "REAL", nullable: true),
                    DistanceKm = table.Column<float>(type: "REAL", nullable: true),
                    DurationSecs = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.ExerciseId);
                    table.ForeignKey(
                        name: "FK_Exercises_WorkoutSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "WorkoutSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityScores_UserId",
                table: "ActivityScores",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeParticipants_UserId",
                table: "ChallengeParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_CreatorId",
                table: "Challenges",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterStates_UserId",
                table: "CharacterStates",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_SessionId",
                table: "Exercises",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_AddresseeId",
                table: "Friendships",
                column: "AddresseeId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_RequesterId_AddresseeId",
                table: "Friendships",
                columns: new[] { "RequesterId", "AddresseeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameStats_UserId",
                table: "GameStats",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_UserId",
                table: "WorkoutSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_XPTransactions_UserId",
                table: "XPTransactions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityScores");

            migrationBuilder.DropTable(
                name: "ChallengeParticipants");

            migrationBuilder.DropTable(
                name: "CharacterStates");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "Friendships");

            migrationBuilder.DropTable(
                name: "GameStats");

            migrationBuilder.DropTable(
                name: "UserAchievements");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "XPTransactions");

            migrationBuilder.DropTable(
                name: "Challenges");

            migrationBuilder.DropTable(
                name: "WorkoutSessions");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
