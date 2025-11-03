using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations;

/// <inheritdoc />
public partial class InitClassesAndMemberships : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Check if index exists before dropping
        migrationBuilder.Sql(@"
            DO $$ 
            BEGIN
                IF EXISTS (
                    SELECT 1 
                    FROM pg_indexes 
                    WHERE indexname = 'IX_Classes_Code'
                ) THEN
                    DROP INDEX ""IX_Classes_Code"";
                END IF;
            END $$;
        ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Only recreate if it doesn't exist
        migrationBuilder.Sql(@"
            DO $$ 
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 
                    FROM pg_indexes 
                    WHERE indexname = 'IX_Classes_Code'
                ) THEN
                    CREATE UNIQUE INDEX ""IX_Classes_Code"" ON ""Classes"" (""Code"");
                END IF;
            END $$;
        ");
    }
}
