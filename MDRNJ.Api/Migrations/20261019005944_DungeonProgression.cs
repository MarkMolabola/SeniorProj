using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MDRNJ.Api.Migrations
{
    /// <inheritdoc />
    public partial class DungeonProgression : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DungeonBossDamage",
                table: "GameStats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "DungeonBossDefeated",
                table: "GameStats",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DungeonBossWeek",
                table: "GameStats",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DungeonFloor",
                table: "GameStats",
                type: "int",
                nullable: false,
                defaultValue: 1); // everyone starts on floor 1

            migrationBuilder.AddColumn<int>(
                name: "DungeonFloorKills",
                table: "GameStats",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DungeonBossDamage",
                table: "GameStats");

            migrationBuilder.DropColumn(
                name: "DungeonBossDefeated",
                table: "GameStats");

            migrationBuilder.DropColumn(
                name: "DungeonBossWeek",
                table: "GameStats");

            migrationBuilder.DropColumn(
                name: "DungeonFloor",
                table: "GameStats");

            migrationBuilder.DropColumn(
                name: "DungeonFloorKills",
                table: "GameStats");
        }
    }
}
