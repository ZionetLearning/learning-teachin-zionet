using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.Migrations;

/// <inheritdoc />
public partial class AddRefreshSessionsTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "refresh_sessions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                refresh_token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                device_fingerprint_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_refresh_sessions", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_refresh_sessions_device_fingerprint_hash",
            table: "refresh_sessions",
            column: "device_fingerprint_hash");

        migrationBuilder.CreateIndex(
            name: "IX_refresh_sessions_expires_at",
            table: "refresh_sessions",
            column: "expires_at");

        migrationBuilder.CreateIndex(
            name: "IX_refresh_sessions_refresh_token_hash",
            table: "refresh_sessions",
            column: "refresh_token_hash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_refresh_sessions_revoked_at",
            table: "refresh_sessions",
            column: "revoked_at");

        migrationBuilder.CreateIndex(
            name: "IX_refresh_sessions_user_id",
            table: "refresh_sessions",
            column: "user_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "refresh_sessions");
    }
}
