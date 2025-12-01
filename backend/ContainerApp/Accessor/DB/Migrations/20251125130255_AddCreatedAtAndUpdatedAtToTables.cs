using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations;

/// <inheritdoc />
public partial class AddCreatedAtAndUpdatedAtToTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "created_at",
            table: "UserProgress",
            type: "timestamp with time zone",
            nullable: false,
            defaultValueSql: "NOW()");

        migrationBuilder.AddColumn<DateTime>(
            name: "created_at",
            table: "UserAchievements",
            type: "timestamp with time zone",
            nullable: false,
            defaultValueSql: "NOW()");

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "UpdatedAt",
            table: "GameAttempts",
            type: "timestamp with time zone",
            nullable: false,
            defaultValueSql: "NOW()");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "created_at",
            table: "UserProgress");

        migrationBuilder.DropColumn(
            name: "created_at",
            table: "UserAchievements");

        migrationBuilder.DropColumn(
            name: "UpdatedAt",
            table: "GameAttempts");
    }
}
