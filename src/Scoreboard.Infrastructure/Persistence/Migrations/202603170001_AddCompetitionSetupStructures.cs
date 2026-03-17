using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scoreboard.Infrastructure.Persistence.Migrations;

public partial class AddCompetitionSetupStructures : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_participants_number",
            table: "participants");

        migrationBuilder.AddColumn<Guid>(
            name: "competition_id",
            table: "participants",
            type: "uuid",
            nullable: false,
            defaultValue: Guid.Empty);

        migrationBuilder.CreateTable(
            name: "run_participants",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                run_id = table.Column<Guid>(type: "uuid", nullable: false),
                participant_id = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_run_participants", x => x.id);
                table.ForeignKey(
                    name: "FK_run_participants_participants_participant_id",
                    column: x => x.participant_id,
                    principalTable: "participants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_run_participants_runs_run_id",
                    column: x => x.run_id,
                    principalTable: "runs",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_participants_competition_id_number",
            table: "participants",
            columns: new[] { "competition_id", "number" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_run_participants_participant_id",
            table: "run_participants",
            column: "participant_id");

        migrationBuilder.CreateIndex(
            name: "IX_run_participants_run_id_participant_id",
            table: "run_participants",
            columns: new[] { "run_id", "participant_id" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_participants_competitions_competition_id",
            table: "participants",
            column: "competition_id",
            principalTable: "competitions",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_participants_competitions_competition_id",
            table: "participants");

        migrationBuilder.DropTable(name: "run_participants");

        migrationBuilder.DropIndex(
            name: "IX_participants_competition_id_number",
            table: "participants");

        migrationBuilder.DropColumn(
            name: "competition_id",
            table: "participants");

        migrationBuilder.CreateIndex(
            name: "IX_participants_number",
            table: "participants",
            column: "number",
            unique: true);
    }
}
