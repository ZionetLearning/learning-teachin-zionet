using Accessor.DB;
using Accessor.Models.Games;
using Accessor.Services;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Accessor.Models.GameConfiguration;


namespace AccessorUnitTests.Services;

public class GameServiceTests
{
    //private const string WordOrderGame = GameName.WordOrder;

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
        var mockMapper = new Mock<IMapper>();

        mockMapper
            .Setup(m => m.Map<SubmitAttemptResult>(It.IsAny<GameAttempt>()))
            .Returns((GameAttempt src) => new SubmitAttemptResult
            {
                StudentId = src.StudentId,
                ExerciseId = src.ExerciseId,
                AttemptId = src.AttemptId,
                GameType = src.GameType,
                Difficulty = src.Difficulty,
                Status = src.Status,
                CorrectAnswer = src.CorrectAnswer,
                AttemptNumber = src.AttemptNumber,
                Accuracy = src.Accuracy
            });

        return new GameService(db, logger, mockMapper.Object);
    }

    #region SubmitAttemptAsync Tests

    [Fact]
    public async Task SubmitAttemptAsync_CorrectAnswer_ReturnsSuccess()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);
        var studentId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var correctAnswer = new List<string> { "שלום", "עולם" };

        db.GameAttempts.Add(new GameAttempt
        {
            AttemptId = exerciseId, // For pending attempts, AttemptId == ExerciseId
            ExerciseId = exerciseId,
            StudentId = studentId,
            GameType = GameName.WordOrder,
            Difficulty = Difficulty.Easy,
            CorrectAnswer = correctAnswer,
            GivenAnswer = new(),
            Status = AttemptStatus.Pending,
            AttemptNumber = 0,
            Accuracy = 0m,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        // Act
        var result = await service.SubmitAttemptAsync(new SubmitAttemptRequest
        {
            StudentId = studentId,
            ExerciseId = exerciseId,
            GivenAnswer = correctAnswer
        }, CancellationToken.None);

        // Assert
        result.Status.Should().Be(AttemptStatus.Success);
        result.AttemptNumber.Should().Be(1);
        result.Accuracy.Should().Be(100m);
    }

    [Fact]
    public async Task SubmitAttemptAsync_IncorrectAnswer_ReturnsFailure()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);
        var studentId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var correctAnswer = new List<string> { "שלום", "עולם" };

        db.GameAttempts.Add(new GameAttempt
        {
            AttemptId = exerciseId, // For pending attempts, AttemptId == ExerciseId
            ExerciseId = exerciseId,
            StudentId = studentId,
            GameType = GameName.WordOrder,
            Difficulty = Difficulty.Medium,
            CorrectAnswer = correctAnswer,
            GivenAnswer = new(),
            Status = AttemptStatus.Pending,
            AttemptNumber = 0,
            Accuracy = 0m,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        // Act
        var result = await service.SubmitAttemptAsync(new SubmitAttemptRequest
        {
            StudentId = studentId,
            ExerciseId = exerciseId,
            GivenAnswer = new List<string> { "wrong", "answer" }
        }, CancellationToken.None);

        // Assert
        result.Status.Should().Be(AttemptStatus.Failure);
        result.CorrectAnswer.Should().BeEquivalentTo(correctAnswer);
        result.Accuracy.Should().BeGreaterThanOrEqualTo(0m);
    }

    [Fact]
    public async Task SubmitAttemptAsync_InvalidInput_ThrowsException()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SubmitAttemptAsync(new SubmitAttemptRequest
            {
                StudentId = Guid.NewGuid(),
                ExerciseId = Guid.NewGuid(),
                GivenAnswer = null!
            }, CancellationToken.None));
    }

    [Fact]
    public async Task SubmitAttemptAsync_NonexistentAttemptId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);

        var studentId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.SubmitAttemptAsync(new SubmitAttemptRequest
            {
                StudentId = studentId,
                ExerciseId = exerciseId,
                GivenAnswer = new List<string> { "test" }
            }, CancellationToken.None));

        exception.Message.Should().Contain("not found");
        exception.Message.Should().Contain(exerciseId.ToString());
        exception.Message.Should().Contain(studentId.ToString());
    }

    [Fact]
    public async Task SubmitAttemptAsync_PartiallyCorrect_CalculatesAccuracy()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);
        var studentId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var correctAnswer = new List<string> { "שלום", "עולם", "יפה", "מאוד" };

        db.GameAttempts.Add(new GameAttempt
        {
            AttemptId = exerciseId, // For pending attempts, AttemptId == ExerciseId
            ExerciseId = exerciseId,
            StudentId = studentId,
            GameType = GameName.WordOrder,
            Difficulty = Difficulty.Easy,
            CorrectAnswer = correctAnswer,
            GivenAnswer = new(),
            Status = AttemptStatus.Pending,
            AttemptNumber = 0,
            Accuracy = 0m,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        // Act - 2 out of 4 words correct in position
        var result = await service.SubmitAttemptAsync(new SubmitAttemptRequest
        {
            StudentId = studentId,
            ExerciseId = exerciseId,
            GivenAnswer = new List<string> { "שלום", "עולם", "wrong", "word" }
        }, CancellationToken.None);

        // Assert
        result.Status.Should().Be(AttemptStatus.Failure);
        result.Accuracy.Should().Be(50m); // 2/4 = 50%
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
                ExerciseId = Guid.NewGuid(),
                StudentId = studentId,
                GameType = GameName.WordOrder,
                Difficulty = Difficulty.Easy,
                CorrectAnswer = new List<string> { "a" },
                GivenAnswer = new List<string> { "a" },
                Status = AttemptStatus.Success,
                AttemptNumber = 1,
                Accuracy = 100m,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                ExerciseId = Guid.NewGuid(),
                StudentId = otherStudentId,
                GameType = GameName.WordOrder,
                Difficulty = Difficulty.Easy,
                CorrectAnswer = new List<string> { "b" },
                GivenAnswer = new List<string> { "b" },
                Status = AttemptStatus.Success,
                AttemptNumber = 1,
                Accuracy = 100m,
                CreatedAt = DateTimeOffset.UtcNow
            }
        );
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetHistoryAsync(studentId, summary: false, page: 1, pageSize: 10, getPending: false, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsDetailed.Should().BeTrue();
        result.Detailed.Should().NotBeNull();
        result.Detailed!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetHistoryAsync_InvalidPagination_UsesDefaults()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);

        // Act
        var result = await service.GetHistoryAsync(Guid.NewGuid(), summary: false, page: -1, pageSize: 200, getPending: false, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsDetailed.Should().BeTrue();
        result.Detailed.Should().NotBeNull();
        result.Detailed!.Page.Should().Be(1);
        result.Detailed.PageSize.Should().Be(100);
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
        var exerciseId1 = Guid.NewGuid();
        var exerciseId2 = Guid.NewGuid();
        var correctAnswer1 = new List<string> { "שלום", "עולם" };
        var correctAnswer2 = new List<string> { "אני", "אוהב" };

        db.GameAttempts.AddRange(
            // Exercise 1 - only failure (should be included)
            new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                ExerciseId = exerciseId1,
                StudentId = studentId,
                GameType = GameName.WordOrder,
                Difficulty = Difficulty.Easy,
                CorrectAnswer = correctAnswer1,
                GivenAnswer = new List<string> { "עולם", "שלום" },
                Status = AttemptStatus.Failure,
                AttemptNumber = 1,
                Accuracy = 0m,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10)
            },
            // Exercise 2 - failure then success (should NOT be included)
            new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                ExerciseId = exerciseId2,
                StudentId = studentId,
                GameType = GameName.WordOrder,
                Difficulty = Difficulty.Medium,
                CorrectAnswer = correctAnswer2,
                GivenAnswer = new List<string> { "אוהב", "אני" },
                Status = AttemptStatus.Failure,
                AttemptNumber = 1,
                Accuracy = 0m,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
            },
            new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                ExerciseId = exerciseId2,
                StudentId = studentId,
                GameType =  GameName.WordOrder,
                Difficulty = Difficulty.Medium,
                CorrectAnswer = correctAnswer2,
                GivenAnswer = correctAnswer2,
                Status = AttemptStatus.Success,
                AttemptNumber = 2,
                Accuracy = 100m,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-2)
            }
        );
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetMistakesAsync(studentId, page: 1, pageSize: 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1); // Only exercise1 should be included (exercise2 has a success)
        var exercise1 = result.Items.First();
        exercise1.ExerciseId.Should().Be(exerciseId1);
        exercise1.CorrectAnswer.Should().BeEquivalentTo(correctAnswer1);
        exercise1.Mistakes.Should().HaveCount(1);
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
                ExerciseId = Guid.NewGuid(),
                StudentId = studentId,
                GameType = GameName.WordOrder,
                Difficulty = Difficulty.Easy,
                CorrectAnswer = correctAnswer,
                GivenAnswer = new List<string> { "wrong" },
                Status = AttemptStatus.Failure,
                AttemptNumber = 1,
                Accuracy = 0m,
                CreatedAt = DateTimeOffset.UtcNow
            },
            new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                ExerciseId = Guid.NewGuid(),
                StudentId = otherStudentId,
                GameType = GameName.WordOrder,
                Difficulty = Difficulty.Easy,
                CorrectAnswer = correctAnswer,
                GivenAnswer = new List<string> { "other" },
                Status = AttemptStatus.Failure,
                AttemptNumber = 1,
                Accuracy = 0m,
                CreatedAt = DateTimeOffset.UtcNow
            }
        );
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetMistakesAsync(studentId, page: 1, pageSize: 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1); // Only 1 exercise for this student
        result.Items.First().Mistakes.Should().HaveCount(1); // With 1 mistake
    }

    [Fact]
    public async Task GetMistakesAsync_GroupsMultipleAttemptsForSameExercise()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);
        var studentId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var correctAnswer = new List<string> { "שלום", "עולם" };
        var attemptId1 = Guid.NewGuid();
        var attemptId2 = Guid.NewGuid();

        db.GameAttempts.AddRange(
            new GameAttempt
            {
                AttemptId = attemptId1,
                ExerciseId = exerciseId,
                StudentId = studentId,
                GameType = GameName.WordOrder,
                Difficulty = Difficulty.Easy,
                CorrectAnswer = correctAnswer,
                GivenAnswer = new List<string> { "עולם", "שלום" },
                Status = AttemptStatus.Failure,
                AttemptNumber = 1,
                Accuracy = 0m,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10)
            },
            new GameAttempt
            {
                AttemptId = attemptId2,
                ExerciseId = exerciseId,
                StudentId = studentId,
                GameType = GameName.WordOrder,
                Difficulty = Difficulty.Easy,
                CorrectAnswer = correctAnswer,
                GivenAnswer = new List<string> { "wrong", "order" },
                Status = AttemptStatus.Failure,
                AttemptNumber = 2,
                Accuracy = 0m,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
            }
        );
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetMistakesAsync(studentId, page: 1, pageSize: 10, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1); // 1 exercise
        var exercise = result.Items.First();
        exercise.ExerciseId.Should().Be(exerciseId);
        exercise.CorrectAnswer.Should().BeEquivalentTo(correctAnswer);
        exercise.Mistakes.Should().HaveCount(2); // 2 failed attempts for the same exercise
        exercise.Mistakes.Should().Contain(m => m.AttemptId == attemptId1);
        exercise.Mistakes.Should().Contain(m => m.AttemptId == attemptId2);
    }

    [Fact]
    public async Task GetMistakesAsync_ExcludesExercisesWithSuccessAfterFailures()
    {
        // Arrange
        var db = NewDb(Guid.NewGuid().ToString());
        var service = NewGameService(db);
        var studentId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var correctAnswer = new List<string> { "שלום", "עולם" };

        db.GameAttempts.AddRange(
            // First attempt - failure
            new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                ExerciseId = exerciseId,
                StudentId = studentId,
                GameType = GameName.WordOrder,
                Difficulty = Difficulty.Easy,
                CorrectAnswer = correctAnswer,
                GivenAnswer = new List<string> { "עולם", "שלום" },
                Status = AttemptStatus.Failure,
                AttemptNumber = 1,
                Accuracy = 0m,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10)
            },
            // Second attempt - another failure
            new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                ExerciseId = exerciseId,
                StudentId = studentId,
                GameType = GameName.WordOrder,
                Difficulty = Difficulty.Easy,
                CorrectAnswer = correctAnswer,
                GivenAnswer = new List<string> { "wrong", "order" },
                Status = AttemptStatus.Failure,
                AttemptNumber = 2,
                Accuracy = 0m,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
            },
            // Third attempt - success
            new GameAttempt
            {
                AttemptId = Guid.NewGuid(),
                ExerciseId = exerciseId,
                StudentId = studentId,
                GameType = GameName.WordOrder,
                Difficulty = Difficulty.Easy,
                CorrectAnswer = correctAnswer,
                GivenAnswer = correctAnswer,
                Status = AttemptStatus.Success,
                AttemptNumber = 3,
                Accuracy = 100m,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-2)
            }
        );
        await db.SaveChangesAsync();

        // Act
        var result = await service.GetMistakesAsync(studentId, page: 1, pageSize: 10, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty(); // Should not include this exercise since it was eventually solved
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
            GameType = GameName.WordOrder,
            Difficulty = Difficulty.Easy,
            Sentences = new List<GeneratedSentenceItem>
                    {
                        new GeneratedSentenceItem
                        {
                            Text = "שלום עולם",
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
        savedAttempts.First().GameType.Should().Be(GameName.WordOrder); // GameType is now an enum
    }
    #endregion
}