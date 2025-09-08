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
    private static readonly Guid IdChatSystem = Guid.Parse("4b2b6d6b-3a3e-4a40-9fb2-9f0b1f2a7c10");
    private static readonly Guid IdChatTitleGenerate = Guid.Parse("e2a0f74f-9d1b-4f2c-8c8a-15a6b7dcb3a2");
    private static readonly Guid IdSystemDefault = Guid.Parse("5f6d7e8f-90a1-4b2c-9d3e-4f5a6b7c8d9e");
    private static readonly Guid IdToneFriendly = Guid.Parse("0a1b2c3d-4e5f-6071-8293-94a5b6c7d8e9");
    private static readonly Guid IdExplanationDetailed = Guid.Parse("de305d54-75b4-431b-adb2-eb6b9e546014");
    private const string VersionIso = "2025-09-07T00:00:00.000Z";

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

        migrationBuilder.InsertData("Prompts", insertColumns, new object[] { IdChatSystem, "chat.system", VersionIso,
        "You are a helpful assistant. Provide accurate, concise, context-aware answers." });

        migrationBuilder.InsertData("Prompts", insertColumns, new object[] { IdChatTitleGenerate, "chat.title.generate", VersionIso,
        """
        You are a naming assistant. Create a short, specific chat title that captures the main topic.
        Rules:
        - Language: match the user's recent messages language.
        - ≤ 6 words, ≤ 50 characters.
        - No quotes, emojis, hashtags, brackets, file names, or PII.
        - Title case for English; sentence case for Russian/others.
        Return STRICT JSON: {"title":"..."}
        """ });

        migrationBuilder.InsertData("Prompts", insertColumns, new object[] { IdSystemDefault, "prompts.system.default", VersionIso,
        "You are a helpful assistant. Maintain context. Keep your answers brief, clear and helpful." });

        migrationBuilder.InsertData("Prompts", insertColumns, new object[] { IdToneFriendly, "prompts.tone.friendly", VersionIso,
        "Speak in a friendly manner, as if you were speaking to a colleague." });

        migrationBuilder.InsertData("Prompts", insertColumns, new object[] { IdExplanationDetailed, "prompts.explanation.detailed", VersionIso,
        "Let's go into detail, step by step, so that even a beginner can understand." });

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
