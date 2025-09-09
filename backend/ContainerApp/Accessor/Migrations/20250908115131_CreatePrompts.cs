using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.Migrations;

/// <inheritdoc />
public partial class CreatePrompts : Migration
{
    private static readonly string[] columns = new[] { "PromptKey", "Version" };
    private static readonly bool[] descending = new[] { false, true };
    private static readonly string[] value = new[] { "Content" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Prompts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PromptKey = table.Column<string>(
                    type: "character varying(120)",
                    maxLength: 120,
                    nullable: false
                ),
                Version = table.Column<string>(
                    type: "character varying(50)",
                    maxLength: 50,
                    nullable: false
                ),
                Content = table.Column<string>(type: "text", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Prompts", x => x.Id);
            }
        );

        migrationBuilder.CreateIndex(
            name: "IX_Prompts_PromptKey_Version_DESC",
            table: "Prompts",
            columns: columns,
            descending: descending
        );

        migrationBuilder
            .CreateIndex(
                name: "IX_Prompts_Covering",
                table: "Prompts",
                columns: columns,
                descending: descending
            )
            .Annotation("Npgsql:Include", value);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Prompts");
    }
}
