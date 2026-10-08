using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MDRNJ.Api.Migrations
{
    /// <inheritdoc />
    public partial class GoldAndGear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Gold",
                table: "GameStats",
                type: "int",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<string>(
                name: "AccessoryGear",
                table: "CharacterStates",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BottomGear",
                table: "CharacterStates",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeadGear",
                table: "CharacterStates",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShoesGear",
                table: "CharacterStates",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TopGear",
                table: "CharacterStates",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserGear",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    PricePaid = table.Column<int>(type: "int", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGear", x => new { x.UserId, x.ItemId });
                    table.ForeignKey(
                        name: "FK_UserGear_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGear");

            migrationBuilder.DropColumn(
                name: "Gold",
                table: "GameStats");

            migrationBuilder.DropColumn(
                name: "AccessoryGear",
                table: "CharacterStates");

            migrationBuilder.DropColumn(
                name: "BottomGear",
                table: "CharacterStates");

            migrationBuilder.DropColumn(
                name: "HeadGear",
                table: "CharacterStates");

            migrationBuilder.DropColumn(
                name: "ShoesGear",
                table: "CharacterStates");

            migrationBuilder.DropColumn(
                name: "TopGear",
                table: "CharacterStates");
        }
    }
}
