using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scoreboard.Infrastructure.Persistence.Migrations;

public partial class AddHeatConfiguredRunCount : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "configured_run_count",
            table: "heats",
            type: "integer",
            nullable: false,
            defaultValue: 1);

        migrationBuilder.AddCheckConstraint(
            name: "ck_heats_configured_run_count",
            table: "heats",
            sql: "configured_run_count > 0");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "ck_heats_configured_run_count",
            table: "heats");

        migrationBuilder.DropColumn(
            name: "configured_run_count",
            table: "heats");
    }
}
