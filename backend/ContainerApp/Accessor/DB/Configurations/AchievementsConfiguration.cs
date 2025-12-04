using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Accessor.Models.Achievements;

namespace Accessor.DB.Configurations;

public class AchievementsConfiguration : IEntityTypeConfiguration<AchievementModel>
{
    public void Configure(EntityTypeBuilder<AchievementModel> builder)
    {
        builder.ToTable("Achievements");

        builder.HasKey(a => a.AchievementId);

        builder.Property(a => a.AchievementId)
            .HasColumnName("achievement_id")
            .IsRequired();

        builder.Property(a => a.Key)
            .HasColumnName("key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .IsRequired();

        builder.Property(a => a.Type)
            .HasColumnName("type")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(a => a.Feature)
            .HasColumnName("feature")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(a => a.TargetCount)
            .HasColumnName("target_count")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasIndex(a => a.Key).IsUnique();
        builder.HasIndex(a => a.Feature);

        // Seed data
        builder.HasData(
            // Word Cards
            new AchievementModel { AchievementId = Guid.Parse("10000000-0000-0000-0000-000000000001"), Key = "word_cards_first", Name = "First Steps", Description = "Complete your first word card", Type = AchievementType.Milestone, Feature = PracticeFeature.WordCards, TargetCount = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new AchievementModel { AchievementId = Guid.Parse("10000000-0000-0000-0000-000000000002"), Key = "word_cards_3", Name = "Word Explorer", Description = "Complete 3 word cards", Type = AchievementType.Count, Feature = PracticeFeature.WordCards, TargetCount = 3, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new AchievementModel { AchievementId = Guid.Parse("10000000-0000-0000-0000-000000000003"), Key = "word_cards_5", Name = "Word Master", Description = "Complete 5 word cards", Type = AchievementType.Count, Feature = PracticeFeature.WordCards, TargetCount = 5, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // Typing Practice
            new AchievementModel { AchievementId = Guid.Parse("20000000-0000-0000-0000-000000000001"), Key = "typing_first", Name = "Keyboard Warrior", Description = "Complete your first typing practice", Type = AchievementType.Milestone, Feature = PracticeFeature.TypingPractice, TargetCount = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new AchievementModel { AchievementId = Guid.Parse("20000000-0000-0000-0000-000000000002"), Key = "typing_3", Name = "Speed Typer", Description = "Complete 3 typing practices", Type = AchievementType.Count, Feature = PracticeFeature.TypingPractice, TargetCount = 3, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new AchievementModel { AchievementId = Guid.Parse("20000000-0000-0000-0000-000000000003"), Key = "typing_5", Name = "Typing Champion", Description = "Complete 5 typing practices", Type = AchievementType.Count, Feature = PracticeFeature.TypingPractice, TargetCount = 5, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // Speaking Practice
            new AchievementModel { AchievementId = Guid.Parse("30000000-0000-0000-0000-000000000001"), Key = "speaking_first", Name = "Breaking the Ice", Description = "Complete your first speaking practice", Type = AchievementType.Milestone, Feature = PracticeFeature.SpeakingPractice, TargetCount = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new AchievementModel { AchievementId = Guid.Parse("30000000-0000-0000-0000-000000000002"), Key = "speaking_3", Name = "Confident Speaker", Description = "Complete 3 speaking practices", Type = AchievementType.Count, Feature = PracticeFeature.SpeakingPractice, TargetCount = 3, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new AchievementModel { AchievementId = Guid.Parse("30000000-0000-0000-0000-000000000003"), Key = "speaking_5", Name = "Voice Master", Description = "Complete 5 speaking practices", Type = AchievementType.Count, Feature = PracticeFeature.SpeakingPractice, TargetCount = 5, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // Word Order
            new AchievementModel { AchievementId = Guid.Parse("40000000-0000-0000-0000-000000000001"), Key = "word_order_first", Name = "Sentence Builder", Description = "Complete your first word order game", Type = AchievementType.Milestone, Feature = PracticeFeature.WordOrder, TargetCount = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new AchievementModel { AchievementId = Guid.Parse("40000000-0000-0000-0000-000000000002"), Key = "word_order_3", Name = "Grammar Guru", Description = "Complete 3 word order games", Type = AchievementType.Count, Feature = PracticeFeature.WordOrder, TargetCount = 3, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new AchievementModel { AchievementId = Guid.Parse("40000000-0000-0000-0000-000000000003"), Key = "word_order_5", Name = "Syntax Master", Description = "Complete 5 word order games", Type = AchievementType.Count, Feature = PracticeFeature.WordOrder, TargetCount = 5, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // Practice Mistakes
            new AchievementModel { AchievementId = Guid.Parse("50000000-0000-0000-0000-000000000001"), Key = "mistakes_first", Name = "Learning from Errors", Description = "Practice your first mistake", Type = AchievementType.Milestone, Feature = PracticeFeature.PracticeMistakes, TargetCount = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new AchievementModel { AchievementId = Guid.Parse("50000000-0000-0000-0000-000000000002"), Key = "mistakes_3", Name = "Mistake Master", Description = "Practice 3 mistakes", Type = AchievementType.Count, Feature = PracticeFeature.PracticeMistakes, TargetCount = 3, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new AchievementModel { AchievementId = Guid.Parse("50000000-0000-0000-0000-000000000003"), Key = "mistakes_5", Name = "Error Eliminator", Description = "Practice 5 mistakes", Type = AchievementType.Count, Feature = PracticeFeature.PracticeMistakes, TargetCount = 5, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

            // Word Cards Challenge
            new AchievementModel { AchievementId = Guid.Parse("60000000-0000-0000-0000-000000000001"), Key = "challenge_first", Name = "Challenge Accepted", Description = "Complete your first word cards challenge", Type = AchievementType.Milestone, Feature = PracticeFeature.WordCardsChallenge, TargetCount = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new AchievementModel { AchievementId = Guid.Parse("60000000-0000-0000-0000-000000000002"), Key = "challenge_3", Name = "Challenge Champion", Description = "Complete 3 word cards challenges", Type = AchievementType.Count, Feature = PracticeFeature.WordCardsChallenge, TargetCount = 3, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new AchievementModel { AchievementId = Guid.Parse("60000000-0000-0000-0000-000000000003"), Key = "challenge_5", Name = "Ultimate Challenger", Description = "Complete 5 word cards challenges", Type = AchievementType.Count, Feature = PracticeFeature.WordCardsChallenge, TargetCount = 5, IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
