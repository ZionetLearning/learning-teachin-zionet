using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations;

/// <inheritdoc />
public partial class AddTeacherStudents : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
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

        migrationBuilder.CreateIndex(
            name: "IX_TeacherStudents_StudentId",
            table: "TeacherStudents",
            column: "StudentId");

        migrationBuilder.CreateIndex(
            name: "IX_TeacherStudents_TeacherId",
            table: "TeacherStudents",
            column: "TeacherId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TeacherStudents");
    }
}
