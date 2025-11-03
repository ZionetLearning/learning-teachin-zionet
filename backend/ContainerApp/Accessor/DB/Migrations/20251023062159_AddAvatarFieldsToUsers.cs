using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations;

/// <inheritdoc />
public partial class AddAvatarFieldsToUsers : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "AvatarContentType",
            table: "Users",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "AvatarPath",
            table: "Users",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "AvatarUpdatedAtUtc",
            table: "Users",
            type: "timestamp with time zone",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AvatarContentType",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "AvatarPath",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "AvatarUpdatedAtUtc",
            table: "Users");
    }
}
