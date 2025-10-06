using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Accessor.DB.Migrations;

/// <inheritdoc />
#pragma warning disable CA1861
public partial class InitialClean : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ChatHistorySnapshots",
            columns: table => new
            {
                ThreadId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                ChatType = table.Column<string>(type: "text", nullable: false, defaultValue: "default"),
                History = table.Column<string>(type: "jsonb", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChatHistorySnapshots", x => x.ThreadId);
            });

        migrationBuilder.CreateTable(
            name: "GameAttempts",
            columns: table => new
            {
                AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                GameType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Difficulty = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                CorrectAnswer = table.Column<List<string>>(type: "jsonb", nullable: false),
                GivenAnswer = table.Column<List<string>>(type: "jsonb", nullable: false),
                IsSuccess = table.Column<bool>(type: "boolean", nullable: false),
                AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_GameAttempts", x => x.AttemptId);
            });

        migrationBuilder.CreateTable(
            name: "Prompts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                PromptKey = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Content = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Prompts", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "refreshSessions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                refresh_token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                device_fingerprint_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                issued_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "NOW() + INTERVAL '60 days'"),
                last_seen_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                ip = table.Column<IPAddress>(type: "inet", nullable: false),
                user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_refreshSessions", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "Tasks",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "text", nullable: false),
                Payload = table.Column<string>(type: "text", nullable: false),
                xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tasks", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TeacherStudents",
            columns: table => new
            {
                TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                StudentId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TeacherStudents", x => new { x.TeacherId, x.StudentId });
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                FirstName = table.Column<string>(type: "text", nullable: false),
                LastName = table.Column<string>(type: "text", nullable: false),
                Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                Role = table.Column<string>(type: "text", nullable: false),
                PreferredLanguageCode = table.Column<string>(type: "text", nullable: false, defaultValue: "en"),
                HebrewLevelValue = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.UserId);
            });

        migrationBuilder.CreateIndex(
            name: "IX_GameAttempts_StudentId",
            table: "GameAttempts",
            column: "StudentId");

        migrationBuilder.CreateIndex(
            name: "IX_GameAttempts_StudentId_GameType_Difficulty",
            table: "GameAttempts",
            columns: new[] { "StudentId", "GameType", "Difficulty" });

        migrationBuilder.CreateIndex(
            name: "IX_GameAttempts_StudentId_IsSuccess",
            table: "GameAttempts",
            columns: new[] { "StudentId", "IsSuccess" });

        migrationBuilder.CreateIndex(
            name: "IX_Prompts_PromptKey",
            table: "Prompts",
            column: "PromptKey");

        migrationBuilder.CreateIndex(
            name: "IX_Prompts_PromptKey_Version",
            table: "Prompts",
            columns: new[] { "PromptKey", "Version" });

        migrationBuilder.CreateIndex(
            name: "IX_refreshSessions_device_fingerprint_hash",
            table: "refreshSessions",
            column: "device_fingerprint_hash");

        migrationBuilder.CreateIndex(
            name: "IX_refreshSessions_expires_at",
            table: "refreshSessions",
            column: "expires_at");

        migrationBuilder.CreateIndex(
            name: "IX_refreshSessions_refresh_token_hash",
            table: "refreshSessions",
            column: "refresh_token_hash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_refreshSessions_revoked_at",
            table: "refreshSessions",
            column: "revoked_at");

        migrationBuilder.CreateIndex(
            name: "IX_refreshSessions_user_id",
            table: "refreshSessions",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "IX_TeacherStudents_StudentId",
            table: "TeacherStudents",
            column: "StudentId");

        migrationBuilder.CreateIndex(
            name: "IX_TeacherStudents_TeacherId",
            table: "TeacherStudents",
            column: "TeacherId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_Role",
            table: "Users",
            column: "Role");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ChatHistorySnapshots");

        migrationBuilder.DropTable(
            name: "GameAttempts");

        migrationBuilder.DropTable(
            name: "Prompts");

        migrationBuilder.DropTable(
            name: "refreshSessions");

        migrationBuilder.DropTable(
            name: "Tasks");

        migrationBuilder.DropTable(
            name: "TeacherStudents");

        migrationBuilder.DropTable(
            name: "Users");
    }
}
#pragma warning restore CA1861