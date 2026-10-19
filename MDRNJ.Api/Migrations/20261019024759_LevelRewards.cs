using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MDRNJ.Api.Migrations
{
    /// <inheritdoc />
    public partial class LevelRewards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnspentStatPoints",
                table: "GameStats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Softer level curve (AccountProgression): level N now needs (N-1)² × 50 total XP instead of × 100,
            // so existing players move up. They get the level-up gold for the new levels (level × 50 each,
            // GoldRules.ForLevelUp) and the free stat points for every level they hold (5 + level each).
            migrationBuilder.Sql(@"
SELECT StatsId, UserId, CurrentLevel AS OldLevel,
       CASE WHEN NewLevel > CurrentLevel THEN NewLevel ELSE CurrentLevel END AS NewLevel
INTO #Levels
FROM (SELECT StatsId, UserId, CurrentLevel,
             CAST(FLOOR(SQRT(TotalXP / 50.0) + 0.000001) AS int) + 1 AS NewLevel
      FROM GameStats) x;

INSERT INTO GoldTransactions (TransactionId, UserId, Amount, Reason, Description, CreatedAt)
SELECT NEWID(), UserId,
       50 * (NewLevel * (NewLevel + 1) / 2 - OldLevel * (OldLevel + 1) / 2),
       4, CONCAT('Level rewards update: now level ', NewLevel), SYSUTCDATETIME()
FROM #Levels WHERE NewLevel > OldLevel;

UPDATE g SET
    Gold = g.Gold + 50 * (l.NewLevel * (l.NewLevel + 1) / 2 - l.OldLevel * (l.OldLevel + 1) / 2),
    CurrentLevel = l.NewLevel,
    UnspentStatPoints = 5 * (l.NewLevel - 1) + l.NewLevel * (l.NewLevel + 1) / 2 - 1
FROM GameStats g JOIN #Levels l ON l.StatsId = g.StatsId;

DROP TABLE #Levels;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Levels and back-paid gold stay: rolling back only drops the unspent points
            migrationBuilder.DropColumn(
                name: "UnspentStatPoints",
                table: "GameStats");
        }
    }
}
