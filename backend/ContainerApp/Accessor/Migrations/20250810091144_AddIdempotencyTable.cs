using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.Migrations;

/// <inheritdoc />
public partial class AddIdempotencyTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Idempotency",
            columns: table => new
            {
                IdempotencyKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Idempotency", x => x.IdempotencyKey);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Idempotency");
    }
}

