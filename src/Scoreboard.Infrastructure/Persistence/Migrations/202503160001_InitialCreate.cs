using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scoreboard.Infrastructure.Persistence.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "competitions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                competition_date = table.Column<DateOnly>(type: "date", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_competitions", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "participants",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                number = table.Column<int>(type: "integer", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_participants", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "heats",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                competition_id = table.Column<Guid>(type: "uuid", nullable: false),
                sequence_number = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_heats", x => x.id);
                table.ForeignKey(
                    name: "FK_heats_competitions_competition_id",
                    column: x => x.competition_id,
                    principalTable: "competitions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "runs",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                heat_id = table.Column<Guid>(type: "uuid", nullable: false),
                sequence_number = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_runs", x => x.id);
                table.ForeignKey(
                    name: "FK_runs_heats_heat_id",
                    column: x => x.heat_id,
                    principalTable: "heats",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "score_entries",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                run_id = table.Column<Guid>(type: "uuid", nullable: false),
                participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                rings = table.Column<int>(type: "integer", nullable: false),
                registered_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_score_entries", x => x.id);
                table.CheckConstraint("ck_score_entries_rings", "rings >= 0 AND rings <= 2");
                table.ForeignKey(
                    name: "FK_score_entries_participants_participant_id",
                    column: x => x.participant_id,
                    principalTable: "participants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_score_entries_runs_run_id",
                    column: x => x.run_id,
                    principalTable: "runs",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_heats_competition_id_sequence_number",
            table: "heats",
            columns: new[] { "competition_id", "sequence_number" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_participants_number",
            table: "participants",
            column: "number",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_runs_heat_id_sequence_number",
            table: "runs",
            columns: new[] { "heat_id", "sequence_number" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_score_entries_participant_id",
            table: "score_entries",
            column: "participant_id");

        migrationBuilder.CreateIndex(
            name: "IX_score_entries_run_id_participant_id",
            table: "score_entries",
            columns: new[] { "run_id", "participant_id" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "score_entries");
        migrationBuilder.DropTable(name: "participants");
        migrationBuilder.DropTable(name: "runs");
        migrationBuilder.DropTable(name: "heats");
        migrationBuilder.DropTable(name: "competitions");
    }
}
