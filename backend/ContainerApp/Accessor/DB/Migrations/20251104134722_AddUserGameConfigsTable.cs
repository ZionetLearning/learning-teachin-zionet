using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations;

/// <inheritdoc />
public partial class AddUserGameConfigsTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "UserGameConfigs",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                GameName = table.Column<string>(type: "text", nullable: false),
                Difficulty = table.Column<string>(type: "text", nullable: false),
                Nikud = table.Column<bool>(type: "boolean", nullable: false),
                NumberOfSentences = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserGameConfigs", x => new { x.UserId, x.GameName });
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "UserGameConfigs");
    }
}
