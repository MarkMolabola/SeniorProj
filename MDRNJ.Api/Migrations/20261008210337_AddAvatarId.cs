using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MDRNJ.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarId",
                table: "CharacterStates",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "classic");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "CharacterStates");
        }
    }
}
