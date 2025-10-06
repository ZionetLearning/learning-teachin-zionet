using Accessor.DB;
using Accessor.Models.Games;
using Accessor.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace AccessorUnitTests.Services;

public class GameServiceTests
{
    private static AccessorDbContext NewDb(string name)
    {
        var options = new DbContextOptionsBuilder<AccessorDbContext>()
            .UseInMemoryDatabase(name)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .EnableSensitiveDataLogging()
            .Options;

        return new AccessorDbContext(options);
    }

    private static GameService NewGameService(AccessorDbContext db)
    {
        var logger = Mock.Of<ILogger<GameService>>();
        return new GameService(db, logger);
    }

    #region SubmitAttemptAsync Tests

    [Fact]
    public async Task SubmitAttemptAsync_CorrectAnswer_ReturnsSuccess()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);
        var studentId = Guid.NewGuid();
        var attemptId = Guid.NewGuid();
        var correctAnswer = new List<string> { "שלום", "עולם" };

        db.GameAttempts.Add(new GameAttempt
        {
            AttemptId = attemptId,
            StudentId = studentId,
            GameType = "wordOrderGame",
            Difficulty = Difficulty.Easy,
            CorrectAnswer = correctAnswer,
            GivenAnswer = new(),
            Status = AttemptStatus.Pending,
            AttemptNumber = 0,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        // Act
        var result = await service.SubmitAttemptAsync(new SubmitAttemptRequest
        {
            StudentId = studentId,
            AttemptId = attemptId,
            GivenAnswer = correctAnswer
        }, CancellationToken.None);

        // Assert
        result.Status.Should().Be(AttemptStatus.Success);
        result.AttemptNumber.Should().Be(1);
    }

    [Fact]
    public async Task SubmitAttemptAsync_IncorrectAnswer_ReturnsFailure()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);
        var studentId = Guid.NewGuid();
        var attemptId = Guid.NewGuid();
        var correctAnswer = new List<string> { "שלום", "עולם" };

        db.GameAttempts.Add(new GameAttempt
        {
            AttemptId = attemptId,
            StudentId = studentId,
            GameType = "wordOrderGame",
            Difficulty = Difficulty.Medium,
            CorrectAnswer = correctAnswer,
            GivenAnswer = new(),
            Status = AttemptStatus.Pending,
            AttemptNumber = 0,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        // Act
        var result = await service.SubmitAttemptAsync(new SubmitAttemptRequest
        {
            StudentId = studentId,
            AttemptId = attemptId,
            GivenAnswer = new List<string> { "עולם", "שלום" }
        }, CancellationToken.None);

        // Assert
        result.Status.Should().Be(AttemptStatus.Failure);
        result.CorrectAnswer.Should().BeEquivalentTo(correctAnswer);
    }

    [Fact]
    public async Task SubmitAttemptAsync_InvalidInput_ThrowsException()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            service.SubmitAttemptAsync(new SubmitAttemptRequest
            {
                StudentId = Guid.NewGuid(),
                AttemptId = Guid.NewGuid(),
                GivenAnswer = null!
            }, CancellationToken.None));
    }

    [Fact]
    public async Task SubmitAttemptAsync_NonexistentId_ThrowsInvalidOperationException()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.SubmitAttemptAsync(new SubmitAttemptRequest
            {
                StudentId = Guid.NewGuid(),
                AttemptId = Guid.NewGuid(),
                GivenAnswer = new List<string> { "test" }
            }, CancellationToken.None));
        
        exception.Message.Should().Contain("No pending attempt found");
    }

    #endregion

    #region GetHistoryAsync Tests

    [Fact]
    public async Task GetHistoryAsync_FiltersStudentData()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);
        var studentId = Guid.NewGuid();
        var otherStudentId = Guid.NewGuid();

        db.GameAttempts.AddRange(
            new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                StudentId = studentId,
                GameType = "wordOrderGame",
                Difficulty = Difficulty.Easy,
                CorrectAnswer = new List<string> { "a" },
                GivenAnswer = new List<string> { "a" },
                Status = AttemptStatus.Success,
                AttemptNumber = 1,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                StudentId = otherStudentId,
                GameType = "wordOrderGame",
                Difficulty = Difficulty.Easy,
                CorrectAnswer = new List<string> { "b" },
                GivenAnswer = new List<string> { "b" },
                Status = AttemptStatus.Success,
                AttemptNumber = 1,
                CreatedAt = DateTimeOffset.UtcNow
            }
        );
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetHistoryAsync(studentId, summary: false, page: 1, pageSize: 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetHistoryAsync_InvalidPagination_UsesDefaults()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);

        // Act
        var result = await service.GetHistoryAsync(Guid.NewGuid(), summary: false, page: -1, pageSize: 200, CancellationToken.None);

        // Assert
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(100);
    }

    #endregion

    #region GetMistakesAsync Tests

    [Fact]
    public async Task GetMistakesAsync_OnlyReturnsFailuresWithoutSuccess()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);
        var studentId = Guid.NewGuid();
        var correctAnswer1 = new List<string> { "שלום", "עולם" };
        var correctAnswer2 = new List<string> { "אני", "לומד" };

        db.GameAttempts.AddRange(
            new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                StudentId = studentId,
                GameType = "wordOrderGame",
                Difficulty = Difficulty.Easy,
                CorrectAnswer = correctAnswer1,
                GivenAnswer = new List<string> { "עולם", "שלום" },
                Status = AttemptStatus.Failure,
                AttemptNumber = 1,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10)
            },
            new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                StudentId = studentId,
                GameType = "wordOrderGame",
                Difficulty = Difficulty.Medium,
                CorrectAnswer = correctAnswer2,
                GivenAnswer = new List<string> { "לומד", "אני" },
                Status = AttemptStatus.Failure,
                AttemptNumber = 1,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
            },
            new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                StudentId = studentId,
                GameType = "wordOrderGame",
                Difficulty = Difficulty.Medium,
                CorrectAnswer = correctAnswer2,
                GivenAnswer = correctAnswer2,
                Status = AttemptStatus.Success,
                AttemptNumber = 2,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-2)
            }
        );
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetMistakesAsync(studentId, page: 1, pageSize: 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items.First().CorrectAnswer.Should().BeEquivalentTo(correctAnswer1);
    }

    [Fact]
    public async Task GetMistakesAsync_FiltersStudentData()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);
        var studentId = Guid.NewGuid();
        var otherStudentId = Guid.NewGuid();
        var correctAnswer = new List<string> { "test" };

        db.GameAttempts.AddRange(
            new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                StudentId = studentId,
                GameType = "wordOrderGame",
                Difficulty = Difficulty.Easy,
                CorrectAnswer = correctAnswer,
                GivenAnswer = new List<string> { "wrong" },
                Status = AttemptStatus.Failure,
                AttemptNumber = 1,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                StudentId = otherStudentId,
                GameType = "wordOrderGame",
                Difficulty = Difficulty.Easy,
                CorrectAnswer = correctAnswer,
                GivenAnswer = new List<string> { "other" },
                Status = AttemptStatus.Failure,
                AttemptNumber = 1,
                CreatedAt = DateTimeOffset.UtcNow
            }
        );
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetMistakesAsync(studentId, page: 1, pageSize: 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
    }

    #endregion

    #region SaveGeneratedSentencesAsync Tests

    [Fact]
    public async Task SaveGeneratedSentencesAsync_SavesAllSentences()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);
        var studentId = Guid.NewGuid();

        var result = await service.SaveGeneratedSentencesAsync(new GeneratedSentenceDto
        {
            StudentId = studentId,
            GameType = "wordOrderGame",
            Difficulty = Difficulty.Easy,
            Sentences = new List<GeneratedSentenceItem>
            {
                new GeneratedSentenceItem
                {
                    Original = "שלום עולם",
                    CorrectAnswer = new List<string> { "שלום", "עולם" },
                    Nikud = true
                }
            }
        }, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var savedAttempts = await db.GameAttempts.Where(a => a.StudentId == studentId).ToListAsync();
        savedAttempts.Should().HaveCount(1);
        savedAttempts.First().Status.Should().Be(AttemptStatus.Pending);
    }

    #endregion
}
