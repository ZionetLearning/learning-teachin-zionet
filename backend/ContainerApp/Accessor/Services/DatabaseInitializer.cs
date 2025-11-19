using Accessor.DB;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;

public class DatabaseInitializer
{
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly AccessorDbContext _dbContext;

    public DatabaseInitializer(AccessorDbContext dbContext, ILogger<DatabaseInitializer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Applying EF Core migrations...");

        var conn = _dbContext.Database.GetDbConnection();
        await conn.OpenAsync();

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT pg_advisory_lock(727274);";
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            await _dbContext.Database.MigrateAsync();
            _logger.LogInformation("Database migration completed.");

            await SeedAchievementsAsync();
        }
        finally
        {
            await using (var unlock = conn.CreateCommand())
            {
                unlock.CommandText = "SELECT pg_advisory_unlock(727274);";
                await unlock.ExecuteNonQueryAsync();
            }
        }
    }

    private async Task SeedAchievementsAsync()
    {
        if (await _dbContext.Achievements.AnyAsync())
        {
            _logger.LogInformation("Achievements already seeded, skipping.");
            return;
        }

        _logger.LogInformation("Seeding achievements...");

        var achievements = new List<Models.Achievements.AchievementModel>
        {
            // Word Cards
            new() { AchievementId = Guid.NewGuid(), Key = "word_cards_first", Name = "First Steps", Description = "Complete your first word card", Type = Models.Achievements.AchievementType.Milestone, Feature = Models.Achievements.PracticeFeature.WordCards, TargetCount = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { AchievementId = Guid.NewGuid(), Key = "word_cards_3", Name = "Word Explorer", Description = "Complete 3 word cards", Type = Models.Achievements.AchievementType.Count, Feature = Models.Achievements.PracticeFeature.WordCards, TargetCount = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { AchievementId = Guid.NewGuid(), Key = "word_cards_5", Name = "Word Master", Description = "Complete 5 word cards", Type = Models.Achievements.AchievementType.Count, Feature = Models.Achievements.PracticeFeature.WordCards, TargetCount = 5, IsActive = true, CreatedAt = DateTime.UtcNow },

            // Typing Practice
            new() { AchievementId = Guid.NewGuid(), Key = "typing_first", Name = "Keyboard Warrior", Description = "Complete your first typing practice", Type = Models.Achievements.AchievementType.Milestone, Feature = Models.Achievements.PracticeFeature.TypingPractice, TargetCount = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { AchievementId = Guid.NewGuid(), Key = "typing_3", Name = "Speed Typer", Description = "Complete 3 typing practices", Type = Models.Achievements.AchievementType.Count, Feature = Models.Achievements.PracticeFeature.TypingPractice, TargetCount = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { AchievementId = Guid.NewGuid(), Key = "typing_5", Name = "Typing Champion", Description = "Complete 5 typing practices", Type = Models.Achievements.AchievementType.Count, Feature = Models.Achievements.PracticeFeature.TypingPractice, TargetCount = 5, IsActive = true, CreatedAt = DateTime.UtcNow },

            // Speaking Practice
            new() { AchievementId = Guid.NewGuid(), Key = "speaking_first", Name = "Breaking the Ice", Description = "Complete your first speaking practice", Type = Models.Achievements.AchievementType.Milestone, Feature = Models.Achievements.PracticeFeature.SpeakingPractice, TargetCount = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { AchievementId = Guid.NewGuid(), Key = "speaking_3", Name = "Confident Speaker", Description = "Complete 3 speaking practices", Type = Models.Achievements.AchievementType.Count, Feature = Models.Achievements.PracticeFeature.SpeakingPractice, TargetCount = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { AchievementId = Guid.NewGuid(), Key = "speaking_5", Name = "Voice Master", Description = "Complete 5 speaking practices", Type = Models.Achievements.AchievementType.Count, Feature = Models.Achievements.PracticeFeature.SpeakingPractice, TargetCount = 5, IsActive = true, CreatedAt = DateTime.UtcNow },

            // Word Order Game
            new() { AchievementId = Guid.NewGuid(), Key = "word_order_first", Name = "Sentence Builder", Description = "Complete your first word order game", Type = Models.Achievements.AchievementType.Milestone, Feature = Models.Achievements.PracticeFeature.WordOrderGame, TargetCount = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { AchievementId = Guid.NewGuid(), Key = "word_order_3", Name = "Grammar Guru", Description = "Complete 3 word order games", Type = Models.Achievements.AchievementType.Count, Feature = Models.Achievements.PracticeFeature.WordOrderGame, TargetCount = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { AchievementId = Guid.NewGuid(), Key = "word_order_5", Name = "Syntax Master", Description = "Complete 5 word order games", Type = Models.Achievements.AchievementType.Count, Feature = Models.Achievements.PracticeFeature.WordOrderGame, TargetCount = 5, IsActive = true, CreatedAt = DateTime.UtcNow },

            // Practice Mistakes
            new() { AchievementId = Guid.NewGuid(), Key = "mistakes_first", Name = "Learning from Errors", Description = "Practice your first mistake", Type = Models.Achievements.AchievementType.Milestone, Feature = Models.Achievements.PracticeFeature.PracticeMistakes, TargetCount = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { AchievementId = Guid.NewGuid(), Key = "mistakes_3", Name = "Mistake Master", Description = "Practice 3 mistakes", Type = Models.Achievements.AchievementType.Count, Feature = Models.Achievements.PracticeFeature.PracticeMistakes, TargetCount = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { AchievementId = Guid.NewGuid(), Key = "mistakes_5", Name = "Error Eliminator", Description = "Practice 5 mistakes", Type = Models.Achievements.AchievementType.Count, Feature = Models.Achievements.PracticeFeature.PracticeMistakes, TargetCount = 5, IsActive = true, CreatedAt = DateTime.UtcNow },

            // Word Cards Challenge
            new() { AchievementId = Guid.NewGuid(), Key = "challenge_first", Name = "Challenge Accepted", Description = "Complete your first word cards challenge", Type = Models.Achievements.AchievementType.Milestone, Feature = Models.Achievements.PracticeFeature.WordCardsChallenge, TargetCount = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { AchievementId = Guid.NewGuid(), Key = "challenge_3", Name = "Challenge Champion", Description = "Complete 3 word cards challenges", Type = Models.Achievements.AchievementType.Count, Feature = Models.Achievements.PracticeFeature.WordCardsChallenge, TargetCount = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { AchievementId = Guid.NewGuid(), Key = "challenge_5", Name = "Ultimate Challenger", Description = "Complete 5 word cards challenges", Type = Models.Achievements.AchievementType.Count, Feature = Models.Achievements.PracticeFeature.WordCardsChallenge, TargetCount = 5, IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        await _dbContext.Achievements.AddRangeAsync(achievements);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} achievements successfully.", achievements.Count);
    }
}