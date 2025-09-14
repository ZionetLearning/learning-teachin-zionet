using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.Migrations;

/// <inheritdoc />
public partial class AddInterestsToUsers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<List<string>>(
            name: "Interests",
            table: "Users",
            type: "jsonb",
            nullable: false,
            defaultValueSql: "'[]'::jsonb");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Interests",
            table: "Users");
    }
}
