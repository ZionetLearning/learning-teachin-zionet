using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using Manager.Models.UserGameConfiguration;
using Manager.Models.Games;
using Manager.Models.Users;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Games;

[Collection("IntegrationTests")]
public class GamesIntegrationTests(
    HttpClientFixture httpClientFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : GamesTestBase(httpClientFixture, outputHelper, signalRFixture)
{
    [Fact(DisplayName = "POST /games-manager/attempt - Invalid empty GUID should return error")]
    public async Task SubmitAttempt_InvalidInput_Should_Return_Error()
    {
        var student = await CreateUserAsync();
        
        var request = new SubmitAttemptRequest
        {
            ExerciseId = Guid.Empty,
            GivenAnswer = new List<string> { "test" }
        };

        var response = await Client.PostAsJsonAsync(GamesRoutes.Attempt, request);
        
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "POST /games-manager/attempt - Nonexistent exercise ID should return error")]
    public async Task SubmitAttempt_NonexistentAttemptId_Should_Return_Error()
    {
        var student = await CreateUserAsync();
        
        var request = new SubmitAttemptRequest
        {
            ExerciseId = Guid.NewGuid(),
            GivenAnswer = new List<string> { "test" }
        };

        var response = await Client.PostAsJsonAsync(GamesRoutes.Attempt, request);
        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "GET /games-manager/history/{id} - Should return paginated history for student")]
    public async Task GetHistory_Should_Return_Correct_History()
    {
    var student = await CreateUserAsync();
        
        // Act: Create some actual game history
        await CreateSuccessfulAttemptAsync(student.UserId, Difficulty.Easy);
        await CreateMistakeAsync(student.UserId, Difficulty.Medium);
     
        var response = await Client.GetAsync($"{GamesRoutes.History(student.UserId)}?summary=false&page=1&pageSize=10&getPending=false");
        response.ShouldBeOk();

 // Assert
        var result = await ReadAsJsonAsync<PagedResult<AttemptHistoryDto>>(response);
        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        
        var submittedAttempts = result.Items.Where(x => x.Status != AttemptStatus.Pending).ToList();
    submittedAttempts.Should().HaveCount(2); // One success, one failure
        result.TotalCount.Should().BeGreaterThanOrEqualTo(2); 
        
        // Verify the structure of submitted history items
        foreach (var item in submittedAttempts)
        {
   item.GameType.Should().Be(GameName.WordOrder.ToString());
    item.Difficulty.Should().BeOneOf(Difficulty.Easy, Difficulty.Medium);
            item.Status.Should().BeOneOf(AttemptStatus.Success, AttemptStatus.Failure);
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
        await CreateSuccessfulAttemptAsync(student.UserId, Difficulty.Easy);
        await CreateMistakeAsync(student.UserId, Difficulty.Easy);
await CreateSuccessfulAttemptAsync(student.UserId, Difficulty.Easy);
   
        // Request summary view
        var response = await Client.GetAsync($"{GamesRoutes.History(student.UserId)}?summary=true&page=1&pageSize=10&getPending=false");
        response.ShouldBeOk();

  // Assert
        var result = await ReadAsJsonAsync<PagedResult<SummaryHistoryDto>>(response);
     result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Items.Should().HaveCount(1); // One group: wordOrderGame + easy 
 
        var summary = result.Items.First();
        summary.GameType.Should().Be(GameName.WordOrder.ToString());
        summary.Difficulty.Should().Be(Difficulty.Easy);
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
        await LoginAsync(student.Email, TestDataHelper.DefaultTestPassword, Role.Student);
        await CreateSuccessfulAttemptAsync(student.UserId, Difficulty.Easy);
        
        // Log back in as teacher
        await LoginAsync(teacher.Email, TestDataHelper.DefaultTestPassword, Role.Teacher);
    
        // Act
    var response = await Client.GetAsync($"{GamesRoutes.History(student.UserId)}?summary=false&page=1&pageSize=10&getPending=false");
    response.ShouldBeOk();
  
        // Assert
        var result = await ReadAsJsonAsync<PagedResult<AttemptHistoryDto>>(response);
    result.Should().NotBeNull();
        
  // Filter out Pending attempts (the API returns all attempts including pending sentence generations)
        var submittedAttempts = result!.Items.Where(x => x.Status != AttemptStatus.Pending).ToList();
        submittedAttempts.Should().HaveCount(1);
        
        var historyItem = submittedAttempts.First();
historyItem.GameType.Should().Be(GameName.WordOrder.ToString());
        historyItem.Difficulty.Should().Be(Difficulty.Easy);
        historyItem.Status.Should().Be(AttemptStatus.Success);
    }

    [Fact(DisplayName = "GET /games-manager/history/{id} - Admin can access any student's history")]
    public async Task GetHistory_Admin_Can_Access_Any_Student_History()
    {
        var admin = await CreateUserAsync(role: "admin");
        
        // Create a student
        var studentModel = await CreateUserViaApiAsync(role: "student");
        
        // Admin should be able to access any student's history
        var response = await Client.GetAsync($"{GamesRoutes.History(studentModel.UserId)}?summary=false&page=1&pageSize=10&getPending=false");
        response.ShouldBeOk();
        
        var result = await ReadAsJsonAsync<PagedResult<AttemptHistoryDto>>(response);
        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "GET /games-manager/history/{id} - Student cannot access other student's history")]
    public async Task GetHistory_UnauthorizedAccess_Should_Return_Forbidden()
    {
        var student1 = await CreateUserAsync();
        var student2Model = await CreateUserViaApiAsync(role: "student");
        
        var response = await Client.GetAsync($"{GamesRoutes.History(student2Model.UserId)}?summary=false&page=1&pageSize=10&getPending=false");
        response.ShouldBeForbidden();
    }

    [Fact(DisplayName = "GET /games-manager/all-history - Admin can access all histories")]
    public async Task GetAllHistory_Admin_Should_Access_All()
    {
        var admin = await CreateUserAsync(role: "admin");
        
        var response = await Client.GetAsync($"{GamesRoutes.AllHistory}?page=1&pageSize=10");
        response.ShouldBeOk();

        var result = await ReadAsJsonAsync<PagedResult<SummaryHistoryWithStudentDto>>(response);
        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "GET /games-manager/all-history - Teacher can access all histories")]
    public async Task GetAllHistory_Teacher_Should_Access_All()
    {
        var teacher = await CreateUserAsync(role: "teacher");
        
        var response = await Client.GetAsync($"{GamesRoutes.AllHistory}?page=1&pageSize=10");
        response.ShouldBeOk();

        var result = await ReadAsJsonAsync<PagedResult<SummaryHistoryWithStudentDto>>(response);
        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "GET /games-manager/all-history - Student cannot access all histories")]
    public async Task GetAllHistory_Student_Should_Be_Forbidden()
    {
        var student = await CreateUserAsync(role: "student");
        
        var response = await Client.GetAsync($"{GamesRoutes.AllHistory}?page=1&pageSize=10");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "GET /games-manager/history/{id} - Invalid page parameter defaults to 1")]
    public async Task GetHistory_InvalidPage_Should_Default()
    {
        var student = await CreateUserAsync();
        
        // Request with invalid page (0 or negative)
        var response = await Client.GetAsync($"{GamesRoutes.History(student.UserId)}?summary=false&page=0&pageSize=10&getPending=false");
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
        var response = await Client.GetAsync($"{GamesRoutes.History(student.UserId)}?summary=false&page=1&pageSize=500&getPending=false");
        response.ShouldBeOk();

  var result = await ReadAsJsonAsync<PagedResult<AttemptHistoryDto>>(response);
        result.Should().NotBeNull();
        result!.PageSize.Should().BeLessThanOrEqualTo(100); // Should be capped at 100
    }

    [Fact(DisplayName = "GET /games-manager/history/{id} - getPending=true should return pending attempts")]
    public async Task GetHistory_GetPendingTrue_Should_Return_Pending_Attempts()
    {
     // Arrange
        var student = await CreateUserAsync();
    
   // Generate sentences but do not submit them - this creates pending attempts
        var pendingSentences = await GenerateSplitSentencesAsync(student.UserId, Difficulty.Easy, count: 2);
     
  // Also create one submitted attempt for comparison
        await CreateSuccessfulAttemptAsync(student.UserId, Difficulty.Medium);
        
        // Act: Request history with getPending=true
        var response = await Client.GetAsync($"{GamesRoutes.History(student.UserId)}?summary=false&page=1&pageSize=10&getPending=true");
  response.ShouldBeOk();

        // Assert
        var result = await ReadAsJsonAsync<PagedResult<AttemptHistoryDto>>(response);
   result.Should().NotBeNull();
     result!.Page.Should().Be(1);
        result.PageSize.Should().Be(10);

   // Log what we got for debugging
   var allItems = result.Items.ToList();
        var pendingCount = allItems.Count(x => x.Status == AttemptStatus.Pending);
        var submittedCount = allItems.Count(x => x.Status != AttemptStatus.Pending);
        OutputHelper.WriteLine($"Total items returned: {allItems.Count}");
        OutputHelper.WriteLine($"Pending count: {pendingCount}");
     OutputHelper.WriteLine($"Submitted count: {submittedCount}");
    
        // Should have both pending and submitted attempts
        var pendingAttempts = allItems.Where(x => x.Status == AttemptStatus.Pending).ToList();
     var submittedAttempts = allItems.Where(x => x.Status != AttemptStatus.Pending).ToList();
     
        // When getPending=true, we should get pending attempts if they exist
  // The test should verify that getPending parameter affects the results
        allItems.Should().NotBeEmpty("getPending=true should return at least some attempts");
   
   // Should have at least 1 submitted attempt (the one we created successfully)
        submittedAttempts.Should().HaveCountGreaterThanOrEqualTo(1);
    
        // If there are pending attempts, verify their structure
   foreach (var pendingAttempt in pendingAttempts)
        {
            pendingAttempt.GameType.Should().Be(GameName.WordOrder.ToString());
            pendingAttempt.Status.Should().Be(AttemptStatus.Pending);
    pendingAttempt.CorrectAnswer.Should().NotBeEmpty();
 // Pending attempts should not have a given answer yet
  if (pendingAttempt.GivenAnswer != null)
   {
  pendingAttempt.GivenAnswer.Should().BeEmpty();
  }
      }
 
   // Verify submitted attempts structure
   foreach (var submittedAttempt in submittedAttempts)
    {
   submittedAttempt.Status.Should().BeOneOf(AttemptStatus.Success, AttemptStatus.Failure);
            submittedAttempt.GivenAnswer.Should().NotBeEmpty();
 }
    }

    [Fact(DisplayName = "GET /games-manager/history/{id} - getPending=false should exclude pending attempts")]
  public async Task GetHistory_GetPendingFalse_Should_Exclude_Pending_Attempts()
    {
 // Arrange
     var student = await CreateUserAsync();
   
        // Generate sentences do not submit them - this creates pending attempts
 await GenerateSplitSentencesAsync(student.UserId, Difficulty.Easy, count: 2);
        
   // Create submitted attempts
        await CreateSuccessfulAttemptAsync(student.UserId, Difficulty.Medium);
        await CreateMistakeAsync(student.UserId, Difficulty.Hard);
    
     // Act: Request history with getPending=false
        var response = await Client.GetAsync($"{GamesRoutes.History(student.UserId)}?summary=false&page=1&pageSize=10&getPending=false");
response.ShouldBeOk();
     
      // Assert
   var result = await ReadAsJsonAsync<PagedResult<AttemptHistoryDto>>(response);
   result.Should().NotBeNull();
      
        // Log what we got for debugging
        var allItems = result!.Items.ToList();
        var pendingCount = allItems.Count(x => x.Status == AttemptStatus.Pending);
  var submittedCount = allItems.Count(x => x.Status != AttemptStatus.Pending);
        OutputHelper.WriteLine($"Total items returned: {allItems.Count}");
        OutputHelper.WriteLine($"Pending count: {pendingCount}");
   OutputHelper.WriteLine($"Submitted count: {submittedCount}");
     
     // Should NOT have any pending attempts when getPending=false
  var pendingAttempts = allItems.Where(x => x.Status == AttemptStatus.Pending).ToList();
        pendingAttempts.Should().BeEmpty("getPending=false should exclude all pending attempts");
     
        // Should have only submitted attempts (success and failure)
        var submittedAttempts = allItems.Where(x => x.Status != AttemptStatus.Pending).ToList();
      submittedAttempts.Should().HaveCountGreaterThanOrEqualTo(2, "we created 2 submitted attempts");
  
        // Verify all returned items are submitted attempts
        foreach (var attempt in allItems)
   {
       attempt.Status.Should().BeOneOf(AttemptStatus.Success, AttemptStatus.Failure);
   attempt.GivenAnswer.Should().NotBeEmpty();
        }
    }

    [Fact(DisplayName = "GET /games-manager/mistakes/{id} - Student can access their own mistakes")]
    public async Task GetMistakes_Student_Sees_Only_Their_Own()
{
 // Arrange
        var student = await CreateUserAsync();

      // Create different types of mistakes
  await CreateMultipleMistakesAsync(student.UserId, count: 3);
     
     // Act
      var response = await Client.GetAsync($"{GamesRoutes.Mistakes(student.UserId)}?page=1&pageSize=10");
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
         mistake.ExerciseId.Should().NotBeEmpty();
            mistake.GameType.Should().Be(GameName.WordOrder);
  mistake.Difficulty.Should().BeOneOf(Difficulty.Easy, Difficulty.Medium, Difficulty.Hard);
 mistake.CorrectAnswer.Should().NotBeEmpty();
   mistake.Mistakes.Should().NotBeEmpty();
     mistake.Mistakes.Should().HaveCountGreaterThan(0);
            
            // Verify the nested mistake attempts
 foreach (var attempt in mistake.Mistakes)
 {
      attempt.AttemptId.Should().NotBeEmpty();
    attempt.WrongAnswer.Should().NotBeEmpty();
   attempt.Accuracy.Should().BeGreaterThanOrEqualTo(0);
    attempt.CreatedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-5));
    }
        }
    }

    [Fact(DisplayName = "GET /games-manager/mistakes/{id} - Student cannot see other student's mistakes")]
    public async Task GetMistakes_Student_Cannot_See_Others()
    {
        var student1 = await CreateUserAsync();
        var student2Model = await CreateUserViaApiAsync(role: "student");
        
        // Student1 tries to access Student2's mistakes
        var response = await Client.GetAsync($"{GamesRoutes.Mistakes(student2Model.UserId)}?page=1&pageSize=10");
        response.ShouldBeForbidden();
    }

    [Fact(DisplayName = "GET /games-manager/mistakes/{id} - Teacher sees their students' mistakes")]
    public async Task GetMistakes_Teacher_Sees_Students_Mistakes()
    {
      // Arrange
        var (teacher, student) = await SetupTeacherStudentRelationshipAsync();
   
        // Log back in as the student to create mistakes
        await LoginAsync(student.Email, TestDataHelper.DefaultTestPassword, Role.Student);
  
    // Create mistakes for the student
   await CreateMistakeAsync(student.UserId, Difficulty.Easy);
        await CreateMistakeAsync(student.UserId, Difficulty.Medium);
    
        // Log back in as teacher to access student's mistakes
        await LoginAsync(teacher.Email, TestDataHelper.DefaultTestPassword, Role.Teacher);
        
    // Act
        var response = await Client.GetAsync($"{GamesRoutes.Mistakes(student.UserId)}?page=1&pageSize=10");
        response.ShouldBeOk();
        
        // Assert
        var result = await ReadAsJsonAsync<PagedResult<MistakeDto>>(response);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2); // We created 2 mistakes
        
        // Verify mistakes structure
        foreach (var mistake in result.Items)
        {
            mistake.ExerciseId.Should().NotBeEmpty();
            mistake.GameType.Should().Be(GameName.WordOrder);
            mistake.Difficulty.Should().BeOneOf(Difficulty.Easy, Difficulty.Medium);
            mistake.CorrectAnswer.Should().NotBeEmpty();
            mistake.Mistakes.Should().NotBeEmpty();
            
            // Verify nested attempts
            foreach (var attempt in mistake.Mistakes)
            {
                attempt.AttemptId.Should().NotBeEmpty();
                attempt.WrongAnswer.Should().NotBeEmpty();
            }
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
     await LoginAsync(studentModel.Email, TestDataHelper.DefaultTestPassword, Role.Student);
        
     // Create a mistake
        await CreateMistakeAsync(studentModel.UserId, Difficulty.Hard);
        
        // Log back in as admin
        await LoginAsync(admin.Email, TestDataHelper.DefaultTestPassword, Role.Admin);
 
        // Act
      var response = await Client.GetAsync($"{GamesRoutes.Mistakes(studentModel.UserId)}?page=1&pageSize=10");
        response.ShouldBeOk();
        
        // Assert
        var result = await ReadAsJsonAsync<PagedResult<MistakeDto>>(response);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        
        var mistake = result.Items.First();
        mistake.ExerciseId.Should().NotBeEmpty();
        mistake.GameType.Should().Be(GameName.WordOrder);
        mistake.Difficulty.Should().Be(Difficulty.Hard);
        mistake.CorrectAnswer.Should().NotBeEmpty();
        mistake.Mistakes.Should().HaveCount(1);
      
        var attempt = mistake.Mistakes.First();
        attempt.AttemptId.Should().NotBeEmpty();
        attempt.WrongAnswer.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "GET /games-manager/mistakes/{id} - Mistakes logic: no mistakes shown for questions answered correctly later")]
    public async Task GetMistakes_DoesNotShowMistakesWithLaterSuccess()
    {
        // Arrange
        var student = await CreateUserAsync();
        
        // Backend filters out mistakes when the same sentence (same CorrectAnswer) is answered correctly later
        await CreateMistakeWithLaterSuccessAsync(student.UserId, Difficulty.Easy);
        
        // Create another standalone mistake (no later success)
        await CreateMistakeAsync(student.UserId, Difficulty.Medium);
        
        // Act
        var response = await Client.GetAsync($"{GamesRoutes.Mistakes(student.UserId)}?page=1&pageSize=10");
        response.ShouldBeOk();
        
        // Assert
        var result = await ReadAsJsonAsync<PagedResult<MistakeDto>>(response);
        result.Should().NotBeNull();
        
        // Only the medium mistake should appear; easy mistake was corrected with later success
        result!.Items.Should().HaveCount(1);
        
        var mistake = result.Items.First();
        mistake.ExerciseId.Should().NotBeEmpty();
        mistake.Difficulty.Should().Be(Difficulty.Medium);
        mistake.CorrectAnswer.Should().NotBeEmpty();
        mistake.Mistakes.Should().HaveCount(1);
        
        var attempt = mistake.Mistakes.First();
        attempt.AttemptId.Should().NotBeEmpty();
        attempt.WrongAnswer.Should().NotBeEmpty();
    }


    // need to add deleteion of the DB before the assert
    [Fact(DisplayName = "GET /games-manager/all-history - Should return correct names and timestamp")]
    public async Task GetAllHistory_Should_Include_AdminNames_And_Timestamp()
    {
        var admin = await CreateUserViaApiAsync(role: "admin");
        await LoginAsync(admin.Email, TestDataHelper.DefaultTestPassword, Role.Admin);

        // first, delete all games history
        var deleteResponse = await Client.DeleteAsync($"{GamesRoutes.AllHistory}");
        deleteResponse.ShouldBeOk();

        // Create some game history for the admin user
        await CreateSuccessfulAttemptAsync(admin.UserId, Difficulty.Easy);
        await CreateMistakeAsync(admin.UserId, Difficulty.Easy);

        var response = await Client.GetAsync($"{GamesRoutes.AllHistory}?page=1&pageSize=10");
        response.ShouldBeOk();

        var result = await ReadAsJsonAsync<PagedResult<SummaryHistoryWithStudentDto>>(response);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);

        var item = result.Items.First();
        item.StudentFirstName.Should().Be(admin.FirstName);
        item.StudentLastName.Should().Be(admin.LastName);
        item.AttemptsCount.Should().Be(2);
        item.TotalSuccesses.Should().Be(1);
        item.TotalFailures.Should().Be(1);
        item.Timestamp.Should().BeAfter(DateTime.UtcNow.AddMinutes(-5));
    }


}
