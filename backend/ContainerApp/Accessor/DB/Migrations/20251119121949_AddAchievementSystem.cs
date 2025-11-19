using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations;

/// <inheritdoc />
public partial class AddAchievementSystem : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Achievements",
            columns: table => new
            {
                achievement_id = table.Column<Guid>(type: "uuid", nullable: false),
                key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "text", nullable: false),
                type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                feature = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                target_count = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Achievements", x => x.achievement_id);
            });

        migrationBuilder.CreateTable(
            name: "UserAchievements",
            columns: table => new
            {
                user_achievement_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                achievement_id = table.Column<Guid>(type: "uuid", nullable: false),
                unlocked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserAchievements", x => x.user_achievement_id);
            });

        migrationBuilder.CreateTable(
            name: "UserProgress",
            columns: table => new
            {
                user_progress_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                feature = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                count = table.Column<int>(type: "integer", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserProgress", x => x.user_progress_id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Achievements_feature",
            table: "Achievements",
            column: "feature");

        migrationBuilder.CreateIndex(
            name: "IX_Achievements_key",
            table: "Achievements",
            column: "key",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserAchievements_achievement_id",
            table: "UserAchievements",
            column: "achievement_id");

        migrationBuilder.CreateIndex(
            name: "IX_UserAchievements_user_id",
            table: "UserAchievements",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "IX_UserAchievements_user_id_achievement_id",
            table: "UserAchievements",
            columns: ["user_id", "achievement_id"],
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_UserProgress_user_id",
            table: "UserProgress",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "IX_UserProgress_user_id_feature",
            table: "UserProgress",
            columns: ["user_id", "feature"],
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Achievements");

        migrationBuilder.DropTable(
            name: "UserAchievements");

        migrationBuilder.DropTable(
            name: "UserProgress");
    }
}
