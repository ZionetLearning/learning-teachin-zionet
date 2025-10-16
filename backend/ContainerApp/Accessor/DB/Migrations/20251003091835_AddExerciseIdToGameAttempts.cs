using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Accessor.DB.Migrations;

/// <inheritdoc />
public partial class AddExerciseIdToGameAttempts : Migration
{
    private static readonly string[] ExerciseIdAttemptNumberColumns = ["ExerciseId", "AttemptNumber"];

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "ExerciseId",
            table: "GameAttempts",
            type: "uuid",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        // Populate ExerciseId with AttemptId for existing records
        // This assumes existing records are individual exercises
        migrationBuilder.Sql("UPDATE \"GameAttempts\" SET \"ExerciseId\" = \"AttemptId\"");

        migrationBuilder.CreateIndex(
            name: "IX_GameAttempts_ExerciseId",
            table: "GameAttempts",
            column: "ExerciseId");

        migrationBuilder.CreateIndex(
            name: "IX_GameAttempts_ExerciseId_AttemptNumber",
            table: "GameAttempts",
            columns: ExerciseIdAttemptNumberColumns);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_GameAttempts_ExerciseId",
            table: "GameAttempts");

        migrationBuilder.DropIndex(
            name: "IX_GameAttempts_ExerciseId_AttemptNumber",
            table: "GameAttempts");

        migrationBuilder.DropColumn(
            name: "ExerciseId",
            table: "GameAttempts");
    }
}
