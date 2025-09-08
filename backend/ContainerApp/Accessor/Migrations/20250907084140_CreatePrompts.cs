using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.Migrations;

/// <inheritdoc />
public partial class CreatePrompts : Migration
{
    private static readonly string[] columns = new[] { "PromptKey", "Version" };
    private static readonly bool[] descending = new[] { false, true };
    private static readonly string[] value = new[] { "Content" };
    private static readonly string[] insertColumns = new[] { "Id", "PromptKey", "Version", "Content" };

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

        migrationBuilder.InsertData(
            table: "Prompts",
            columns: insertColumns,
            values: new object[] {
                new object[] {
                    Guid.NewGuid(),
                    "chat.system",
                    "2025-09-07T00:00:00.000Z",
                    "You are a helpful assistant. Provide accurate, concise, context-aware answers.",
                },
                new object[] {
                    Guid.NewGuid(),
                    "chat.title.generate",
                    "2025-09-07T00:00:00.000Z",
                    """
            You are a naming assistant. Create a short, specific chat title that captures the main topic.
            
            Rules:
            - Language: match the user's recent messages language.
            - ≤ 6 words, ≤ 50 characters.
            - No quotes, emojis, hashtags, brackets, file names, or PII.
            - Title case for English; sentence case for Russian/others.
            Return STRICT JSON: {"title":"..."}
            """,
                },
                new object[] {
                    Guid.NewGuid(),
                    "prompts.system.default",
                    "2025-09-07T00:00:00.000Z",
                    "You are a helpful assistant. Maintain context. Keep your answers brief, clear and helpful.",
                },
                new object[] {
                    Guid.NewGuid(),
                    "prompts.tone.friendly",
                    "2025-09-07T00:00:00.000Z",
                    "Speak in a friendly manner, as if you were speaking to a colleague.",
                },
                new object[] {
                    Guid.NewGuid(),
                    "prompts.explanation.detailed",
                    "2025-09-07T00:00:00.000Z",
                    "Let's go into detail, step by step, so that even a beginner can understand.",
                },
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
