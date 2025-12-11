using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelSnapshotJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "History",
                table: "ChatHistorySnapshots",
                type: "json",
                nullable: false,
                defaultValue: "null",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldDefaultValue: "null");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "History",
                table: "ChatHistorySnapshots",
                type: "jsonb",
                nullable: false,
                defaultValue: "null",
                oldClrType: typeof(string),
                oldType: "json",
                oldDefaultValue: "null");
        }
    }
}
