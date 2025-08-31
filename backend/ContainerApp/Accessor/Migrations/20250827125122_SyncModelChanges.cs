using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.Migrations;

/// <inheritdoc />
public partial class SyncModelChanges : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "PasswordHash",
            table: "Users",
            newName: "Password");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Password",
            table: "Users",
            newName: "PasswordHash");
    }
}
