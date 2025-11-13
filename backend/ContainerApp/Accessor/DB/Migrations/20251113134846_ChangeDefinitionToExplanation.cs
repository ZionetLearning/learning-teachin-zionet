using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDefinitionToExplanation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Definition",
                table: "WordCards",
                newName: "Explanation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Explanation",
                table: "WordCards",
                newName: "Definition");
        }
    }
}
