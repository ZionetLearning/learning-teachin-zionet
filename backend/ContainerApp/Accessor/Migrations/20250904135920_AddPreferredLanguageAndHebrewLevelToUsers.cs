using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredLanguageAndHebrewLevelToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HebrewLevelValue",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreferredLanguageCode",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HebrewLevelValue",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredLanguageCode",
                table: "Users");
        }
    }
}
