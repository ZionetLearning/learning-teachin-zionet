using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Accessor.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    private static readonly string[] IxChatMessages_ThreadId_Timestamp = new[] { "ThreadId", "timestamp" };

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ChatThreads",
            columns: table => new
            {
                ThreadId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<string>(type: "text", nullable: false),
                ChatName = table.Column<string>(type: "text", nullable: false),
                ChatType = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChatThreads", x => x.ThreadId);
            });

        migrationBuilder.CreateTable(
            name: "Idempotency",
            columns: table => new
            {
                IdempotencyKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Idempotency", x => x.IdempotencyKey);
            });

        migrationBuilder.CreateTable(
            name: "refreshSessions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                refresh_token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                device_fingerprint_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                issued_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false, defaultValueSql: "NOW()"),
                expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
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
                Payload = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tasks", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ChatMessages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ThreadId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<string>(type: "text", nullable: false),
                Role = table.Column<int>(type: "integer", nullable: false),
                Content = table.Column<string>(type: "text", nullable: false),
                timestamp = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChatMessages", x => x.Id);
                table.ForeignKey(
                    name: "FK_ChatMessages_ChatThreads_ThreadId",
                    column: x => x.ThreadId,
                    principalTable: "ChatThreads",
                    principalColumn: "ThreadId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ChatMessages_ThreadId",
            table: "ChatMessages",
            column: "ThreadId");

        migrationBuilder.CreateIndex(
            name: "IX_ChatMessages_ThreadId_timestamp",
            table: "ChatMessages",
            columns: IxChatMessages_ThreadId_Timestamp);

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
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ChatMessages");

        migrationBuilder.DropTable(
            name: "Idempotency");

        migrationBuilder.DropTable(
            name: "refreshSessions");

        migrationBuilder.DropTable(
            name: "Tasks");

        migrationBuilder.DropTable(
            name: "ChatThreads");
    }
}
