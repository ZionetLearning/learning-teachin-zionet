using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.Migrations;

/// <inheritdoc />
public partial class Add_ChatHistorySnapshot_NameAndDefaults : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ChatMessages");

        migrationBuilder.DropTable(
            name: "ChatThreads");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ChatThreads",
            columns: table => new
            {
                ThreadId = table.Column<Guid>(type: "uuid", nullable: false),
                ChatName = table.Column<string>(type: "text", nullable: false),
                ChatType = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UserId = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChatThreads", x => x.ThreadId);
            });

        migrationBuilder.CreateTable(
            name: "ChatMessages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ThreadId = table.Column<Guid>(type: "uuid", nullable: false),
                Content = table.Column<string>(type: "text", nullable: false),
                Role = table.Column<int>(type: "integer", nullable: false),
                timestamp = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                UserId = table.Column<string>(type: "text", nullable: false)
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

        var columns = new[] { "ThreadId", "timestamp" };
        migrationBuilder.CreateIndex(
            name: "IX_ChatMessages_ThreadId_timestamp",
            table: "ChatMessages",
            columns: columns);
    }
}
