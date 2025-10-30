using Accessor.Models.Meetings;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations;

/// <inheritdoc />
public partial class AddMeetingsTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Meetings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Attendees = table.Column<List<MeetingAttendee>>(type: "jsonb", nullable: false),
                StartTimeUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                GroupCallId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                CreatedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Meetings", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Meetings_CreatedByUserId",
            table: "Meetings",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Meetings_GroupCallId",
            table: "Meetings",
            column: "GroupCallId");

        migrationBuilder.CreateIndex(
            name: "IX_Meetings_StartTimeUtc",
            table: "Meetings",
            column: "StartTimeUtc");

        migrationBuilder.CreateIndex(
            name: "IX_Meetings_Status",
            table: "Meetings",
            column: "Status");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Meetings");
    }
}
