using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations;

/// <inheritdoc />
public partial class AddAcsUserIdToUsers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "AcsUserId",
            table: "Users",
            type: "character varying(255)",
            maxLength: 255,
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_AcsUserId",
            table: "Users",
            column: "AcsUserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Users_AcsUserId",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "AcsUserId",
            table: "Users");
    }
}
