using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations;

/// <inheritdoc />
public partial class InitClassesAndMemberships : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Classes",
            columns: table => new
            {
                ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                Description = table.Column<string>(type: "text", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Classes", x => x.ClassId);
            });

        migrationBuilder.CreateTable(
            name: "ClassMemberships",
            columns: table => new
            {
                ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Role = table.Column<int>(type: "integer", nullable: false),
                AddedBy = table.Column<Guid>(type: "uuid", nullable: false),
                AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ClassMemberships", x => new { x.ClassId, x.UserId, x.Role });
                table.ForeignKey(
                    name: "FK_ClassMemberships_Classes_ClassId",
                    column: x => x.ClassId,
                    principalTable: "Classes",
                    principalColumn: "ClassId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ClassMemberships_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "UserId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Classes_Name_CI",
            table: "Classes",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ClassMemberships_ClassId_Role_UserId",
            table: "ClassMemberships",
            columns: ["ClassId", "Role", "UserId"]);

        migrationBuilder.CreateIndex(
            name: "IX_ClassMemberships_UserId_Role_ClassId",
            table: "ClassMemberships",
            columns: ["UserId", "Role", "ClassId"]);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ClassMemberships");

        migrationBuilder.DropTable(
            name: "Classes");
    }
}
