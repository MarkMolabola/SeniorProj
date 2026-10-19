using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MDRNJ.Api.Migrations
{
    /// <summary>
    /// Special heroes became shop items. Players already playing as one keep it for free.
    /// </summary>
    public partial class GrandfatherHeroes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO UserGear (UserId, ItemId, PricePaid, PurchasedAt)
                SELECT c.UserId, c.AvatarId, 0, SYSUTCDATETIME()
                FROM CharacterStates c
                WHERE c.AvatarId <> 'classic'
                  AND NOT EXISTS (SELECT 1 FROM UserGear g WHERE g.UserId = c.UserId AND g.ItemId = c.AvatarId)
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
