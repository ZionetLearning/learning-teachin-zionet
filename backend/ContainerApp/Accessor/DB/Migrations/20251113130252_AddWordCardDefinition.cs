using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations
{
    /// <inheritdoc />
    public partial class AddWordCardDefinition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Definition",
                table: "WordCards",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Definition",
                table: "WordCards");
        }
    }
}
