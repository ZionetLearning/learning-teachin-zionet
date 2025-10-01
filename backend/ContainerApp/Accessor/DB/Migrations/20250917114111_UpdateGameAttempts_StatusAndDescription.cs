using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations;

public partial class UpdateGameAttempts_StatusAndDescription : Migration
{
    private static readonly string[] StudentId_Status_IndexColumns = { "StudentId", "Status" };
    private static readonly string[] StudentId_IsSuccess_IndexColumns = { "StudentId", "IsSuccess" };

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_GameAttempts_StudentId_IsSuccess",
            table: "GameAttempts");

        migrationBuilder.DropColumn(
            name: "IsSuccess",
            table: "GameAttempts");

        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "GameAttempts",
            type: "character varying(500)",
            maxLength: 500,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "Status",
            table: "GameAttempts",
            type: "character varying(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "IX_GameAttempts_StudentId_Status",
            table: "GameAttempts",
            columns: StudentId_Status_IndexColumns);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_GameAttempts_StudentId_Status",
            table: "GameAttempts");

        migrationBuilder.DropColumn(
            name: "Description",
            table: "GameAttempts");

        migrationBuilder.DropColumn(
            name: "Status",
            table: "GameAttempts");

        migrationBuilder.AddColumn<bool>(
            name: "IsSuccess",
            table: "GameAttempts",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_GameAttempts_StudentId_IsSuccess",
            table: "GameAttempts",
            columns: StudentId_IsSuccess_IndexColumns);
    }
}