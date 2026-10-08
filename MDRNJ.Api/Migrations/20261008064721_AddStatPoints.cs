using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MDRNJ.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStatPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "CardioPoints",
                table: "ActivityScores",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "FlexibilityPoints",
                table: "ActivityScores",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "StrengthPoints",
                table: "ActivityScores",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            // Backfill stat points from workouts already logged, using the
            // StatRules weights as of this migration (0.1 point per weighted minute).
            // Unlisted workout types use the "Other" split 0.3 / 0.5 / 0.2.
            migrationBuilder.Sql("""
                UPDATE a SET
                    a.StrengthPoints    = t.S,
                    a.CardioPoints      = t.C,
                    a.FlexibilityPoints = t.F
                FROM [ActivityScores] a
                JOIN (
                    SELECT w.UserId,
                           SUM(w.DurationMins * ISNULL(x.S, 0.3) * 0.1) AS S,
                           SUM(w.DurationMins * ISNULL(x.C, 0.5) * 0.1) AS C,
                           SUM(w.DurationMins * ISNULL(x.F, 0.2) * 0.1) AS F
                    FROM [WorkoutSessions] w
                    LEFT JOIN (VALUES
                        (0,  1.0, 0.0, 0.0),  -- Weightlifting
                        (1,  0.0, 1.0, 0.0),  -- Running
                        (2,  0.0, 1.0, 0.0),  -- Cycling
                        (3,  0.0, 1.0, 0.0),  -- Swimming
                        (4,  0.2, 0.7, 0.1),  -- Tennis
                        (5,  0.4, 0.6, 0.0),  -- Boxing
                        (6,  0.0, 0.0, 1.0),  -- Yoga
                        (7,  0.4, 0.6, 0.0),  -- HIIT
                        (8,  0.2, 0.7, 0.1),  -- Basketball
                        (9,  0.2, 0.7, 0.1),  -- Football
                        (10, 0.2, 0.4, 0.4),  -- Golf
                        (11, 0.2, 0.8, 0.0),  -- Hiking
                        (12, 0.2, 0.8, 0.0),  -- Rowing
                        (13, 0.6, 0.2, 0.2)   -- Climbing
                    ) AS x(WorkoutType, S, C, F) ON x.WorkoutType = w.WorkoutType
                    WHERE w.EndTime IS NOT NULL
                    GROUP BY w.UserId
                ) t ON t.UserId = a.UserId;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardioPoints",
                table: "ActivityScores");

            migrationBuilder.DropColumn(
                name: "FlexibilityPoints",
                table: "ActivityScores");

            migrationBuilder.DropColumn(
                name: "StrengthPoints",
                table: "ActivityScores");
        }
    }
}
