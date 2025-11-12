using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations;

/// <inheritdoc />
public partial class AddAccuracyToGameAttempts : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "Accuracy",
            table: "GameAttempts",
            type: "numeric",
            nullable: false,
            defaultValue: 0m);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Accuracy",
            table: "GameAttempts");
    }
}
