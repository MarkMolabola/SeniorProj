using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MDRNJ.Api.Migrations
{
    /// <inheritdoc />
    public partial class ChallengeProof : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowDisputes",
                table: "Challenges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ChallengerApproved",
                table: "Challenges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DisputeReason",
                table: "Challenges",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DisputedById",
                table: "Challenges",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OpponentApproved",
                table: "Challenges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireProof",
                table: "Challenges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewDeadline",
                table: "Challenges",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProofPath",
                table: "ChallengeAttempts",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowDisputes",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ChallengerApproved",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "DisputeReason",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "DisputedById",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "OpponentApproved",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "RequireProof",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ReviewDeadline",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "ProofPath",
                table: "ChallengeAttempts");
        }
    }
}
