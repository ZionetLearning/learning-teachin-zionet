using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using IntegrationTests.Models.Games;
using Manager.Models.Users;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Games;

/// <summary>
/// Games integration tests using per-test user isolation.
/// </summary>
[Collection("Per-test user collection")]
public class GamesIntegrationTests(
    PerTestUserFixture perUserFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : GamesTestBase(perUserFixture, outputHelper, signalRFixture), IAsyncLifetime
{
    // Note: Tests for correct/incorrect submissions require the full sentence generation flow
    // These would need to:
    // 1. Generate sentences via POST /ai-manager/sentence/split
    // 2. Wait for SignalR event with AttemptIds
    // 3. Submit attempts with correct/incorrect answers
    // 4. Verify the response status and attempt numbers
    // 
    // For now, these are covered by the end-to-end flow tests in the AI tests

    [Fact(DisplayName = "POST /games-manager/attempt - Invalid empty GUID should return error")]
    public async Task SubmitAttempt_InvalidInput_Should_Return_Error()
    {
        var student = await CreateUserAsync();
        
        var request = new SubmitAttemptRequest
        {
            StudentId = student.UserId,
            AttemptId = Guid.Empty, // Invalid attempt ID
            GivenAnswer = new List<string> { "test" }
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.GameAttempt, request);
        
        // The endpoint returns 500 (Problem) when attempt doesn't exist
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "POST /games-manager/attempt - Nonexistent attempt ID should return error")]
    public async Task SubmitAttempt_NonexistentAttemptId_Should_Return_Error()
    {
        var student = await CreateUserAsync();
        
        var request = new SubmitAttemptRequest
        {
            StudentId = student.UserId,
            AttemptId = Guid.NewGuid(), // Non-existent attempt ID
            GivenAnswer = new List<string> { "test" }
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.GameAttempt, request);
        
        // Should return 500 (Problem) as the attempt doesn't exist
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "POST /games-manager/attempt - Student cannot submit for another student")]
    public async Task SubmitAttempt_UnauthorizedAccess_Should_Return_Forbidden()
    {
        var student1 = await CreateUserAsync();
        var student2 = TestDataHelper.CreateUser(role: "student");
        
        // Create student2 (without logging in as them)
        await Client.PostAsJsonAsync(UserRoutes.UserBase, student2);
        
        // Student1 tries to submit for Student2 (even with a random attemptId)
        var request = new SubmitAttemptRequest
        {
            StudentId = student2.UserId,
            AttemptId = Guid.NewGuid(),
            GivenAnswer = new List<string> { "test" }
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.GameAttempt, request);
        response.ShouldBeForbidden();
    }

    [Fact(DisplayName = "GET /games-manager/history/{id} - Should return paginated history for student")]
    public async Task GetHistory_Should_Return_Correct_History()
    {
        var student = await CreateUserAsync();
        
        // Even with no history, the endpoint should return a valid empty paged result
        var response = await Client.GetAsync($"{ApiRoutes.GameHistory(student.UserId)}?summary=false&page=1&pageSize=10");
        response.ShouldBeOk();

        var result = await ReadAsJsonAsync<PagedResult<AttemptHistoryDto>>(response);
        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact(DisplayName = "GET /games-manager/history/{id} - Student cannot access other student's history")]
    public async Task GetHistory_UnauthorizedAccess_Should_Return_Forbidden()
    {
        var student1 = await CreateUserAsync();
        var student2 = TestDataHelper.CreateUser(role: "student");
        
        // Create student2
        await Client.PostAsJsonAsync(UserRoutes.UserBase, student2);
        
        // Student1 tries to access Student2's history
        var response = await Client.GetAsync($"{ApiRoutes.GameHistory(student2.UserId)}?summary=false&page=1&pageSize=10");
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

    // Note: Testing the logic that mistakes endpoint returns only games with failed attempts and no later success
    // requires creating actual game attempts with failures and successes through the full flow.
    // The GameService logic filters out mistakes where there's a later success for the same question.
    // This is validated by the unit tests for GameService.GetMistakesAsync

    [Fact(DisplayName = "GET /games-manager/mistakes/{id} - Student can access their own mistakes")]
    public async Task GetMistakes_Student_Sees_Only_Their_Own()
    {
        var student = await CreateUserAsync();
        
        // Student should be able to access their own mistakes (even if empty)
        var response = await Client.GetAsync($"{ApiRoutes.GameMistakes(student.UserId)}?page=1&pageSize=10");
        response.ShouldBeOk();
        
        var result = await ReadAsJsonAsync<PagedResult<MistakeDto>>(response);
        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact(DisplayName = "GET /games-manager/mistakes/{id} - Student cannot see other student's mistakes")]
    public async Task GetMistakes_Student_Cannot_See_Others()
    {
        var student1 = await CreateUserAsync();
        var student2 = TestDataHelper.CreateUser(role: "student");
        
        // Create student2
        await Client.PostAsJsonAsync(UserRoutes.UserBase, student2);
        
        // Student1 tries to access Student2's mistakes
        var response = await Client.GetAsync($"{ApiRoutes.GameMistakes(student2.UserId)}?page=1&pageSize=10");
        response.ShouldBeForbidden();
    }

    [Fact(DisplayName = "GET /games-manager/mistakes/{id} - Teacher sees their students' mistakes")]
    public async Task GetMistakes_Teacher_Sees_Students_Mistakes()
    {
        // Login as Admin to set up teacher-student relationship
        var admin = await CreateUserAsync(role: "admin");
        
        // Create teacher and student
        var teacher = TestDataHelper.CreateUser(role: "teacher");
        var student = TestDataHelper.CreateUser(role: "student");
        
        await Client.PostAsJsonAsync(UserRoutes.UserBase, teacher);
        await Client.PostAsJsonAsync(UserRoutes.UserBase, student);
        
        // Assign student to teacher
        await Client.PostAsync(MappingRoutes.Assign(teacher.UserId, student.UserId), null);
        
        // Now login as teacher
        await PerUserFixture.CreateAndLoginAsync(Role.Teacher, teacher.Email);
        
        // Teacher should be able to access their student's mistakes
        var response = await Client.GetAsync($"{ApiRoutes.GameMistakes(student.UserId)}?page=1&pageSize=10");
        response.ShouldBeOk();
    }

    [Fact(DisplayName = "GET /games-manager/mistakes/{id} - Admin sees all mistakes")]
    public async Task GetMistakes_Admin_Sees_All()
    {
        var admin = await CreateUserAsync(role: "admin");
        
        // Create a student
        var student = TestDataHelper.CreateUser(role: "student");
        await Client.PostAsJsonAsync(UserRoutes.UserBase, student);
        
        // Admin should be able to access any student's mistakes
        var response = await Client.GetAsync($"{ApiRoutes.GameMistakes(student.UserId)}?page=1&pageSize=10");
        response.ShouldBeOk();
        
        var result = await ReadAsJsonAsync<PagedResult<MistakeDto>>(response);
        result.Should().NotBeNull();
    }
}
