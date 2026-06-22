using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scoreboard.Infrastructure.Persistence.Migrations;

public partial class AddScoreEntryClientSubmissionId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "client_submission_id",
            table: "score_entries",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_score_entries_client_submission_id",
            table: "score_entries",
            column: "client_submission_id",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_score_entries_client_submission_id",
            table: "score_entries");

        migrationBuilder.DropColumn(
            name: "client_submission_id",
            table: "score_entries");
    }
}
