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

        migrationBuilder.Sql("ALTER TABLE heats ADD CONSTRAINT ck_heats_configured_run_count CHECK (configured_run_count > 0);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE heats DROP CONSTRAINT ck_heats_configured_run_count;");

        migrationBuilder.DropColumn(
            name: "configured_run_count",
            table: "heats");
    }
}
