//using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.Migrations;

/// <inheritdoc />
public partial class AddExpiresAtDefault : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "expires_at",
            table: "refreshSessions",
            type: "timestamptz",
            nullable: false,
            defaultValueSql: "NOW() + INTERVAL '60 days'",
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamptz");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "expires_at",
            table: "refreshSessions",
            type: "timestamptz",
            nullable: false,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamptz",
            oldDefaultValueSql: "NOW() + INTERVAL '60 days'");
    }
}
