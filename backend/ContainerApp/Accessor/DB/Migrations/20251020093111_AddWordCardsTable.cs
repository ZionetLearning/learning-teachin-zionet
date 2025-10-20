using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations;

/// <inheritdoc />
public partial class AddWordCardsTable : Migration
{
    private static readonly string[] columns = new[] { "user_id", "is_learned" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "WordCards",
            columns: table => new
            {
                card_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                hebrew = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                english = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                is_learned = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WordCards", x => x.card_id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_WordCards_user_id_is_learned",
            table: "WordCards",
            columns: columns);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "WordCards");
    }
}
