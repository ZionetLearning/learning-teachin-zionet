using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using IntegrationTests.Models.Games;
using Manager.Models.Users;
using Models.Ai.Sentences;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Games;

[Collection("Per-test user collection")]
public class GamesIntegrationTests(
    PerTestUserFixture perUserFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : GamesTestBase(perUserFixture, outputHelper, signalRFixture), IAsyncLifetime
{
    [Fact(DisplayName = "POST /games-manager/attempt - Invalid empty GUID should return error")]
    public async Task SubmitAttempt_InvalidInput_Should_Return_Error()
    {
        var student = await CreateUserAsync();
        
        var request = new SubmitAttemptRequest
        {
            StudentId = student.UserId,
            AttemptId = Guid.Empty,
            GivenAnswer = new List<string> { "test" }
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.GamesAttempt, request);
        
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "POST /games-manager/attempt - Nonexistent attempt ID should return error")]
    public async Task SubmitAttempt_NonexistentAttemptId_Should_Return_Error()
    {
        var student = await CreateUserAsync();
        
        var request = new SubmitAttemptRequest
        {
            StudentId = student.UserId,
            AttemptId = Guid.NewGuid(),
            GivenAnswer = new List<string> { "test" }
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.GamesAttempt, request);
        
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "POST /games-manager/attempt - Student cannot submit for another student")]
    public async Task SubmitAttempt_UnauthorizedAccess_Should_Return_Forbidden()
    {
        var student1 = await CreateUserAsync();
        var student2 = await CreateUserViaApiAsync(role: "student");
        
        var request = new SubmitAttemptRequest
        {
            StudentId = student2.UserId,
            AttemptId = Guid.NewGuid(),
            GivenAnswer = new List<string> { "test" }
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.GamesAttempt, request);
        response.ShouldBeForbidden();
    }

    [Fact(DisplayName = "GET /games-manager/history/{id} - Should return paginated history for student")]
    public async Task GetHistory_Should_Return_Correct_History()
    {
        var student = await CreateUserAsync();
        
        // Act: Create some actual game history
        await CreateSuccessfulAttemptAsync(student.UserId, Difficulty.easy);
        await CreateMistakeAsync(student.UserId, Difficulty.medium);
        
        var response = await Client.GetAsync($"{ApiRoutes.GameHistory(student.UserId)}?summary=false&page=1&pageSize=10");
        response.ShouldBeOk();

        // Assert
        var result = await ReadAsJsonAsync<PagedResult<AttemptHistoryDto>>(response);
        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        
        var submittedAttempts = result.Items.Where(x => x.Status != "Pending").ToList();
        submittedAttempts.Should().HaveCount(2); // One success, one failure
        result.TotalCount.Should().BeGreaterThanOrEqualTo(2); 
        
        // Verify the structure of submitted history items
        foreach (var item in submittedAttempts)
        {
            item.GameType.Should().Be("wordOrderGame");
            item.Difficulty.ToLower().Should().BeOneOf("easy", "medium");
            item.Status.Should().BeOneOf("Success", "Failure");
            item.CorrectAnswer.Should().NotBeEmpty();
            item.GivenAnswer.Should().NotBeEmpty();
        }
    }

    [Fact(DisplayName = "GET /games-manager/history/{id} - Should return summary history for student")]
    public async Task GetHistory_Summary_Should_Return_Aggregated_Data()
    {
        // Arrange
        var student = await CreateUserAsync();
        
        // Act: Create multiple attempts for the same game type and difficulty
        await CreateSuccessfulAttemptAsync(student.UserId, Difficulty.easy);
        await CreateMistakeAsync(student.UserId, Difficulty.easy);
        await CreateSuccessfulAttemptAsync(student.UserId, Difficulty.easy);
        
        // Request summary view
        var response = await Client.GetAsync($"{ApiRoutes.GameHistory(student.UserId)}?summary=true&page=1&pageSize=10");
        response.ShouldBeOk();

        // Assert
        var result = await ReadAsJsonAsync<PagedResult<SummaryHistoryDto>>(response);
        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().HaveCount(1); // One group: wordOrderGame + easy
        
        var summary = result.Items.First();
        summary.GameType.Should().Be("wordOrderGame");
        summary.Difficulty.ToLower().Should().Be("easy");
        summary.AttemptsCount.Should().Be(3); // 2 successes + 1 failure
        summary.TotalSuccesses.Should().Be(2);
        summary.TotalFailures.Should().Be(1);
    }

    [Fact(DisplayName = "GET /games-manager/history/{id} - Teacher can access their student's history")]
    public async Task GetHistory_Teacher_Can_Access_Student_History()
    {
        // Arrange
        var (teacher, student) = await SetupTeacherStudentRelationshipAsync();
        
        // Log in as student to create some history
        await LoginAsync(student.Email, "Passw0rd!", Role.Student);
        await CreateSuccessfulAttemptAsync(student.UserId, Difficulty.easy);
        
        // Log back in as teacher  
        await LoginAsync(teacher.Email, "Passw0rd!", Role.Teacher);
        
        // Act
        var response = await Client.GetAsync($"{ApiRoutes.GameHistory(student.UserId)}?summary=false&page=1&pageSize=10");
        response.ShouldBeOk();
        
        // Assert
        var result = await ReadAsJsonAsync<PagedResult<AttemptHistoryDto>>(response);
        result.Should().NotBeNull();
        
        // Filter out Pending attempts (the API returns all attempts including pending sentence generations)
        var submittedAttempts = result!.Items.Where(x => x.Status != "Pending").ToList();
        submittedAttempts.Should().HaveCount(1);
        
        var historyItem = submittedAttempts.First();
        historyItem.GameType.Should().Be("wordOrderGame");
        historyItem.Difficulty.ToLower().Should().Be("easy");
        historyItem.Status.Should().Be("Success");
    }

    [Fact(DisplayName = "GET /games-manager/history/{id} - Admin can access any student's history")]
    public async Task GetHistory_Admin_Can_Access_Any_Student_History()
    {
        var admin = await CreateUserAsync(role: "admin");
        
        // Create a student
        var studentModel = await CreateUserViaApiAsync(role: "student");
        
        // Admin should be able to access any student's history
        var response = await Client.GetAsync($"{ApiRoutes.GameHistory(studentModel.UserId)}?summary=false&page=1&pageSize=10");
        response.ShouldBeOk();
        
        var result = await ReadAsJsonAsync<PagedResult<AttemptHistoryDto>>(response);
        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "GET /games-manager/history/{id} - Student cannot access other student's history")]
    public async Task GetHistory_UnauthorizedAccess_Should_Return_Forbidden()
    {
        var student1 = await CreateUserAsync();
        var student2Model = await CreateUserViaApiAsync(role: "student");
        
        var response = await Client.GetAsync($"{ApiRoutes.GameHistory(student2Model.UserId)}?summary=false&page=1&pageSize=10");
        response.ShouldBeForbidden();
    }

    [Fact(DisplayName = "GET /games-manager/all-history - Admin can access all histories")]
    public async Task GetAllHistory_Admin_Should_Access_All()
    {
        var admin = await CreateUserAsync(role: "admin");
        
        var response = await Client.GetAsync($"{ApiRoutes.GameAllHistory}?page=1&pageSize=10");
        response.ShouldBeOk();

        var result = await ReadAsJsonAsync<PagedResult<SummaryHistoryWithStudentDto>>(response);
        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "GET /games-manager/all-history - Teacher can access all histories")]
    public async Task GetAllHistory_Teacher_Should_Access_All()
    {
        var teacher = await CreateUserAsync(role: "teacher");
        
        var response = await Client.GetAsync($"{ApiRoutes.GameAllHistory}?page=1&pageSize=10");
        response.ShouldBeOk();

        var result = await ReadAsJsonAsync<PagedResult<SummaryHistoryWithStudentDto>>(response);
        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "GET /games-manager/all-history - Student cannot access all histories")]
    public async Task GetAllHistory_Student_Should_Be_Forbidden()
    {
        var student = await CreateUserAsync(role: "student");
        
        var response = await Client.GetAsync($"{ApiRoutes.GameAllHistory}?page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "GET /games-manager/history/{id} - Invalid page parameter defaults to 1")]
    public async Task GetHistory_InvalidPage_Should_Default()
    {
        var student = await CreateUserAsync();
        
        // Request with invalid page (0 or negative)
        var response = await Client.GetAsync($"{ApiRoutes.GameHistory(student.UserId)}?summary=false&page=0&pageSize=10");
        response.ShouldBeOk();

        var result = await ReadAsJsonAsync<PagedResult<AttemptHistoryDto>>(response);
        result.Should().NotBeNull();
        result!.Page.Should().BeGreaterThanOrEqualTo(1); // Should default to 1
    }

    [Fact(DisplayName = "GET /games-manager/history/{id} - Large pageSize is capped at 100")]
    public async Task GetHistory_LargePageSize_Should_Be_Capped()
    {
        var student = await CreateUserAsync();
        
        // Request with very large pageSize
        var response = await Client.GetAsync($"{ApiRoutes.GameHistory(student.UserId)}?summary=false&page=1&pageSize=500");
        response.ShouldBeOk();

        var result = await ReadAsJsonAsync<PagedResult<AttemptHistoryDto>>(response);
        result.Should().NotBeNull();
        result!.PageSize.Should().BeLessThanOrEqualTo(100); // Should be capped at 100
    }

    [Fact(DisplayName = "GET /games-manager/mistakes/{id} - Student can access their own mistakes")]
    public async Task GetMistakes_Student_Sees_Only_Their_Own()
    {
        // Arrange
        var student = await CreateUserAsync();
        
        // Create different types of mistakes
        await CreateMultipleMistakesAsync(student.UserId, count: 3);
        
        // Act
        var response = await Client.GetAsync($"{ApiRoutes.GameMistakes(student.UserId)}?page=1&pageSize=10");
        response.ShouldBeOk();
        
        // Assert
        var result = await ReadAsJsonAsync<PagedResult<MistakeDto>>(response);
        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().HaveCount(3);
        
        // Verify each mistake has the expected structure
        foreach (var mistake in result.Items)
        {
            mistake.GameType.Should().Be("wordOrderGame");
            mistake.Difficulty.ToLower().Should().BeOneOf("easy", "medium", "hard");
            mistake.CorrectAnswer.Should().NotBeEmpty();
            mistake.WrongAnswers.Should().NotBeEmpty();
            mistake.WrongAnswers.Should().HaveCountGreaterThan(0);
        }
    }

    [Fact(DisplayName = "GET /games-manager/mistakes/{id} - Student cannot see other student's mistakes")]
    public async Task GetMistakes_Student_Cannot_See_Others()
    {
        var student1 = await CreateUserAsync();
        var student2Model = await CreateUserViaApiAsync(role: "student");
        
        // Student1 tries to access Student2's mistakes
        var response = await Client.GetAsync($"{ApiRoutes.GameMistakes(student2Model.UserId)}?page=1&pageSize=10");
        response.ShouldBeForbidden();
    }

    [Fact(DisplayName = "GET /games-manager/mistakes/{id} - Teacher sees their students' mistakes")]
    public async Task GetMistakes_Teacher_Sees_Students_Mistakes()
    {
        // Arrange
        var (teacher, student) = await SetupTeacherStudentRelationshipAsync();
        
        // Log back in as the student to create mistakes
        await LoginAsync(student.Email, "Passw0rd!", Role.Student);
        
        // Create mistakes for the student
        await CreateMistakeAsync(student.UserId, Difficulty.easy);
        await CreateMistakeAsync(student.UserId, Difficulty.medium);
        
        // Log back in as teacher to access student's mistakes
        await LoginAsync(teacher.Email, "Passw0rd!", Role.Teacher);
        
        // Act
        var response = await Client.GetAsync($"{ApiRoutes.GameMistakes(student.UserId)}?page=1&pageSize=10");
        response.ShouldBeOk();
        
        // Assert
        var result = await ReadAsJsonAsync<PagedResult<MistakeDto>>(response);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2); // We created 2 mistakes
        
        // Verify mistakes structure
        foreach (var mistake in result.Items)
        {
            mistake.GameType.Should().Be("wordOrderGame");
            mistake.Difficulty.ToLower().Should().BeOneOf("easy", "medium");
            mistake.CorrectAnswer.Should().NotBeEmpty();
            mistake.WrongAnswers.Should().NotBeEmpty();
        }
    }

    [Fact(DisplayName = "GET /games-manager/mistakes/{id} - Admin sees all mistakes")]
    public async Task GetMistakes_Admin_Sees_All()
    {
        // Arrange
        var admin = await CreateUserAsync(role: "admin");
        
        // Create a student
        var studentModel = await CreateUserViaApiAsync(role: "student");
        
        // Log in as the student to create mistakes
        await LoginAsync(studentModel.Email, studentModel.Password, Role.Student);
        
        // Create a mistake
        await CreateMistakeAsync(studentModel.UserId, Difficulty.hard);
        
        // Log back in as admin
        await LoginAsync(admin.Email, "Test123!", Role.Admin);
        
        // Act
        var response = await Client.GetAsync($"{ApiRoutes.GameMistakes(studentModel.UserId)}?page=1&pageSize=10");
        response.ShouldBeOk();
        
        // Assert
        var result = await ReadAsJsonAsync<PagedResult<MistakeDto>>(response);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        
        var mistake = result.Items.First();
        mistake.GameType.Should().Be("wordOrderGame");
        mistake.Difficulty.ToLower().Should().Be("hard");
        mistake.CorrectAnswer.Should().NotBeEmpty();
        mistake.WrongAnswers.Should().HaveCount(1);
    }

    [Fact(DisplayName = "GET /games-manager/mistakes/{id} - Mistakes logic: no mistakes shown for questions answered correctly later")]
    public async Task GetMistakes_DoesNotShowMistakesWithLaterSuccess()
    {
        // Arrange
        var student = await CreateUserAsync();
        
        // Backend filters out mistakes when the same sentence (same CorrectAnswer) is answered correctly later
        await CreateMistakeWithLaterSuccessAsync(student.UserId, Difficulty.easy);
        
        // Create another standalone mistake (no later success)
        await CreateMistakeAsync(student.UserId, Difficulty.medium);
        
        // Act
        var response = await Client.GetAsync($"{ApiRoutes.GameMistakes(student.UserId)}?page=1&pageSize=10");
        response.ShouldBeOk();
        
        // Assert
        var result = await ReadAsJsonAsync<PagedResult<MistakeDto>>(response);
        result.Should().NotBeNull();
        
        // Only the medium mistake should appear; easy mistake was corrected with later success
        result!.Items.Should().HaveCount(1);
        
        var mistake = result.Items.First();
        mistake.Difficulty.ToLower().Should().Be("medium");
        mistake.CorrectAnswer.Should().NotBeEmpty();
        mistake.WrongAnswers.Should().HaveCount(1);
    }
}
