using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations;

/// <inheritdoc />
public partial class AddMeetingsTableWithDurationAndDescription : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddCheckConstraint(
            name: "CK_Meetings_DurationMinutes",
            table: "Meetings",
            sql: "\"DurationMinutes\" >= 1 AND \"DurationMinutes\" <= 1440");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropCheckConstraint(
            name: "CK_Meetings_DurationMinutes",
            table: "Meetings");
    }
}
