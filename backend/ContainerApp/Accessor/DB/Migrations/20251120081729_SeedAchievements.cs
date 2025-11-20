using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Accessor.DB.Migrations
{
    /// <inheritdoc />
    public partial class SeedAchievements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "achievement_id", "created_at", "description", "feature", "is_active", "key", "name", "target_count", "type" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Complete your first word card", "WordCards", true, "word_cards_first", "First Steps", 1, "Milestone" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Complete 3 word cards", "WordCards", true, "word_cards_3", "Word Explorer", 3, "Count" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Complete 5 word cards", "WordCards", true, "word_cards_5", "Word Master", 5, "Count" },
                    { new Guid("20000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Complete your first typing practice", "TypingPractice", true, "typing_first", "Keyboard Warrior", 1, "Milestone" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Complete 3 typing practices", "TypingPractice", true, "typing_3", "Speed Typer", 3, "Count" },
                    { new Guid("20000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Complete 5 typing practices", "TypingPractice", true, "typing_5", "Typing Champion", 5, "Count" },
                    { new Guid("30000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Complete your first speaking practice", "SpeakingPractice", true, "speaking_first", "Breaking the Ice", 1, "Milestone" },
                    { new Guid("30000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Complete 3 speaking practices", "SpeakingPractice", true, "speaking_3", "Confident Speaker", 3, "Count" },
                    { new Guid("30000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Complete 5 speaking practices", "SpeakingPractice", true, "speaking_5", "Voice Master", 5, "Count" },
                    { new Guid("40000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Complete your first word order game", "WordOrder", true, "word_order_first", "Sentence Builder", 1, "Milestone" },
                    { new Guid("40000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Complete 3 word order games", "WordOrder", true, "word_order_3", "Grammar Guru", 3, "Count" },
                    { new Guid("40000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Complete 5 word order games", "WordOrder", true, "word_order_5", "Syntax Master", 5, "Count" },
                    { new Guid("50000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Practice your first mistake", "PracticeMistakes", true, "mistakes_first", "Learning from Errors", 1, "Milestone" },
                    { new Guid("50000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Practice 3 mistakes", "PracticeMistakes", true, "mistakes_3", "Mistake Master", 3, "Count" },
                    { new Guid("50000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Practice 5 mistakes", "PracticeMistakes", true, "mistakes_5", "Error Eliminator", 5, "Count" },
                    { new Guid("60000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Complete your first word cards challenge", "WordCardsChallenge", true, "challenge_first", "Challenge Accepted", 1, "Milestone" },
                    { new Guid("60000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Complete 3 word cards challenges", "WordCardsChallenge", true, "challenge_3", "Challenge Champion", 3, "Count" },
                    { new Guid("60000000-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Complete 5 word cards challenges", "WordCardsChallenge", true, "challenge_5", "Ultimate Challenger", 5, "Count" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Achievements",
                keyColumn: "achievement_id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000003"));
        }
    }
}
