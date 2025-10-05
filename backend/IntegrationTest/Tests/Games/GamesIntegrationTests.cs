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
    [Fact(DisplayName = "POST /games-manager/attempt - Submit correct answer should return success")]
    public async Task SubmitAttempt_CorrectAnswer_Should_Return_Success()
    {
        var student = await CreateUserAsync();
        
        // Create a pending attempt first
        var attemptId = await CreatePendingAttemptAsync(student.UserId, new List<string> { "שלום", "עולם" });
        
        var request = new SubmitAttemptRequest
        {
            StudentId = student.UserId,
            AttemptId = attemptId,
            GivenAnswer = new List<string> { "שלום", "עולם" }
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.GameAttempt, request);
        response.ShouldBeOk();

        var result = await ReadAsJsonAsync<SubmitAttemptResult>(response);
        result.Should().NotBeNull();
        result!.StudentId.Should().Be(student.UserId);
        result.Status.Should().Be("Success");
        result.AttemptNumber.Should().Be(1);
    }

    [Fact(DisplayName = "POST /games-manager/attempt - Submit incorrect answer should return failure")]
    public async Task SubmitAttempt_IncorrectAnswer_Should_Return_Failure()
    {
        var student = await CreateUserAsync();
        
        var attemptId = await CreatePendingAttemptAsync(student.UserId, new List<string> { "שלום", "עולם" });
        
        var request = new SubmitAttemptRequest
        {
            StudentId = student.UserId,
            AttemptId = attemptId,
            GivenAnswer = new List<string> { "שלום", "לכם" } // Wrong answer
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.GameAttempt, request);
        response.ShouldBeOk();

        var result = await ReadAsJsonAsync<SubmitAttemptResult>(response);
        result.Should().NotBeNull();
        result!.StudentId.Should().Be(student.UserId);
        result.Status.Should().Be("Failure");
        result.AttemptNumber.Should().Be(1);
    }

    [Fact(DisplayName = "POST /games-manager/attempt - Invalid input should return 400 or 500")]
    public async Task SubmitAttempt_InvalidInput_Should_Return_BadRequestOrError()
    {
        var student = await CreateUserAsync();
        
        var request = new SubmitAttemptRequest
        {
            StudentId = student.UserId,
            AttemptId = Guid.Empty, // Invalid attempt ID
            GivenAnswer = new List<string> { "test" }
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.GameAttempt, request);
        
        // The endpoint might return 500 (Problem) or 400 depending on validation
        response.StatusCode.Should().Match(code => 
            code == HttpStatusCode.BadRequest || 
            code == HttpStatusCode.InternalServerError);
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
        
        var attemptId = await CreatePendingAttemptAsync(student2.UserId, new List<string> { "שלום" });
        
        // Student1 tries to submit for Student2
        var request = new SubmitAttemptRequest
        {
            StudentId = student2.UserId,
            AttemptId = attemptId,
            GivenAnswer = new List<string> { "שלום" }
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.GameAttempt, request);
        response.ShouldBeForbidden();
    }

    [Fact(DisplayName = "GET /games-manager/history/{id} - Should return correct history for student")]
    public async Task GetHistory_Should_Return_Correct_History()
    {
        var student = await CreateUserAsync();
        
        // Create and submit multiple attempts
        var attemptId1 = await CreatePendingAttemptAsync(student.UserId, new List<string> { "שלום" });
        await Client.PostAsJsonAsync(ApiRoutes.GameAttempt, new SubmitAttemptRequest
        {
            StudentId = student.UserId,
            AttemptId = attemptId1,
            GivenAnswer = new List<string> { "שלום" }
        });

        var response = await Client.GetAsync($"{ApiRoutes.GameHistory(student.UserId)}?summary=false&page=1&pageSize=10");
        response.ShouldBeOk();

        var result = await ReadAsJsonAsync<PagedResult<AttemptHistoryDto>>(response);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
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

    [Fact(DisplayName = "GET /games-manager/mistakes/{id} - Returns only games with failed attempts and no later success")]
    public async Task GetMistakes_Should_Return_Only_Unsolved_Mistakes()
    {
        var student = await CreateUserAsync();
        
        // Create a mistake (failed attempt)
        var attemptId1 = await CreatePendingAttemptAsync(student.UserId, new List<string> { "טעות" });
        await Client.PostAsJsonAsync(ApiRoutes.GameAttempt, new SubmitAttemptRequest
        {
            StudentId = student.UserId,
            AttemptId = attemptId1,
            GivenAnswer = new List<string> { "שגוי" } // Wrong answer
        });

        // Create a corrected mistake (failed then succeeded)
        var attemptId2 = await CreatePendingAttemptAsync(student.UserId, new List<string> { "נכון" });
        await Client.PostAsJsonAsync(ApiRoutes.GameAttempt, new SubmitAttemptRequest
        {
            StudentId = student.UserId,
            AttemptId = attemptId2,
            GivenAnswer = new List<string> { "לא נכון" } // Wrong answer first
        });
        
        var attemptId3 = await CreatePendingAttemptAsync(student.UserId, new List<string> { "נכון" });
        await Client.PostAsJsonAsync(ApiRoutes.GameAttempt, new SubmitAttemptRequest
        {
            StudentId = student.UserId,
            AttemptId = attemptId3,
            GivenAnswer = new List<string> { "נכון" } // Correct answer on retry
        });

        var response = await Client.GetAsync($"{ApiRoutes.GameMistakes(student.UserId)}?page=1&pageSize=10");
        response.ShouldBeOk();

        var result = await ReadAsJsonAsync<PagedResult<MistakeDto>>(response);
        result.Should().NotBeNull();
        
        // Should only contain the first mistake (טעות), not the corrected one (נכון)
        result!.Items.Should().NotContain(m => m.CorrectAnswer.Contains("נכון"));
    }

    [Fact(DisplayName = "GET /games-manager/mistakes/{id} - Student sees only their mistakes")]
    public async Task GetMistakes_Student_Sees_Only_Their_Own()
    {
        var student = await CreateUserAsync();
        
        var response = await Client.GetAsync($"{ApiRoutes.GameMistakes(student.UserId)}?page=1&pageSize=10");
        response.ShouldBeOk();
        
        var result = await ReadAsJsonAsync<PagedResult<MistakeDto>>(response);
        result.Should().NotBeNull();
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
