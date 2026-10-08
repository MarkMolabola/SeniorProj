using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MDRNJ.Api.Migrations
{
    /// <inheritdoc />
    public partial class FriendDuels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Old open challenges are replaced outright by 1v1 friend challenges.
            // Their rows don't map onto the new columns, so clear them first.
            migrationBuilder.Sql("DELETE FROM ChallengeParticipants; DELETE FROM Challenges;");

            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_Users_CreatorId",
                table: "Challenges");

            migrationBuilder.DropTable(
                name: "ChallengeParticipants");

            migrationBuilder.DropIndex(
                name: "IX_Challenges_CreatorId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "TargetValue",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Challenges");

            migrationBuilder.RenameColumn(
                name: "XPReward",
                table: "Challenges",
                newName: "Scoring");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Challenges",
                newName: "Deadline");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Challenges",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Challenges",
                newName: "OpponentId");

            migrationBuilder.RenameColumn(
                name: "ChallengeType",
                table: "Challenges",
                newName: "OpponentXP");

            migrationBuilder.AddColumn<Guid>(
                name: "ChallengerId",
                table: "Challenges",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<float>(
                name: "ChallengerScore",
                table: "Challenges",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ChallengerSeenResult",
                table: "Challenges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ChallengerXP",
                table: "Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Contest",
                table: "Challenges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "DistanceKm",
                table: "Challenges",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "OpponentScore",
                table: "Challenges",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OpponentSeenResult",
                table: "Challenges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "Challenges",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WinnerId",
                table: "Challenges",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChallengeAttempts",
                columns: table => new
                {
                    AttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<float>(type: "real", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChallengeAttempts", x => x.AttemptId);
                    table.ForeignKey(
                        name: "FK_ChallengeAttempts_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "ChallengeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_ChallengerId_Status",
                table: "Challenges",
                columns: new[] { "ChallengerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_OpponentId_Status",
                table: "Challenges",
                columns: new[] { "OpponentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeAttempts_ChallengeId_UserId",
                table: "ChallengeAttempts",
                columns: new[] { "ChallengeId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_Users_ChallengerId",
                table: "Challenges",
                column: "ChallengerId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_Users_OpponentId",
                table: "Challenges",
                column: "OpponentId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_Users_ChallengerId",
                table: "Challenges");

            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_Users_OpponentId",
                table: "Challenges");

            migrationBuilder.DropTable(
                name: "ChallengeAttempts");

            migrationBuilder.DropIndex(
                name: "IX_Challenges_ChallengerId_Status",
                table: "Challenges");

            migrationBuilder.DropIndex(
                name: "IX_Challenges_OpponentId_Status",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengerId",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengerScore",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengerSeenResult",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengerXP",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "Contest",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "DistanceKm",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "OpponentScore",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "OpponentSeenResult",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "WinnerId",
                table: "Challenges");

            migrationBuilder.RenameColumn(
                name: "Scoring",
                table: "Challenges",
                newName: "XPReward");

            migrationBuilder.RenameColumn(
                name: "OpponentXP",
                table: "Challenges",
                newName: "ChallengeType");

            migrationBuilder.RenameColumn(
                name: "OpponentId",
                table: "Challenges",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "Deadline",
                table: "Challenges",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Challenges",
                newName: "EndDate");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Challenges",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "TargetValue",
                table: "Challenges",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Challenges",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ChallengeParticipants",
                columns: table => new
                {
                    ChallengeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Progress = table.Column<float>(type: "real", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_CreatorId",
                table: "Challenges",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ChallengeParticipants_UserId",
                table: "ChallengeParticipants",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_Users_CreatorId",
                table: "Challenges",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
