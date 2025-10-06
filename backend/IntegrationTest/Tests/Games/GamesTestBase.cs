using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using IntegrationTests.Constants;
using IntegrationTests.Models.Auth;
using IntegrationTests.Models.Games;
using IntegrationTests.Models.Notification;
using Models.Ai.Sentences;
using Manager.Models.Auth;
using Manager.Models.Users;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Games;

[Collection("Per-test user collection")]
public abstract class GamesTestBase(
    PerTestUserFixture perUserFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBase(perUserFixture.HttpFixture, outputHelper, signalRFixture)
{
    protected PerTestUserFixture PerUserFixture { get; } = perUserFixture;

    /// <summary>
    /// Creates a user (default role: student) and logs them in.
    /// </summary>
    protected Task<UserData> CreateUserAsync(
        string role = "student",
        string? email = null)
    {
        var parsedRole = Enum.TryParse<Role>(role, true, out var r) ? r : Role.Student;
        return PerUserFixture.CreateAndLoginAsync(parsedRole, email);
    }

    /// <summary>
    /// Creates a user via API without logging them in.
    /// Returns the created user model.
    /// </summary>
    protected async Task<CreateUser> CreateUserViaApiAsync(string role = "student", string? email = null)
    {
        var user = TestDataHelper.CreateUser(role: role, email: email);
        var response = await Client.PostAsJsonAsync(UserRoutes.UserBase, user);
        response.EnsureSuccessStatusCode();
        return user;
    }

    /// <summary>
    /// Logs in with existing credentials and sets the bearer token.
    /// Returns UserData for the logged-in user.
    /// </summary>
    protected async Task<UserData> LoginAsync(string email, string password, Role role)
    {
        var loginReq = new LoginRequest { Email = email, Password = password };
        var loginRes = await Client.PostAsJsonAsync(AuthRoutes.Login, loginReq);
        loginRes.EnsureSuccessStatusCode();

        var body = await loginRes.Content.ReadAsStringAsync();
        var tokenRes = JsonSerializer.Deserialize<AccessTokenResponse>(body)
                       ?? throw new InvalidOperationException("Invalid login response");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenRes.AccessToken);

        // Extract UserId from JWT token
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenRes.AccessToken);
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == TestConstants.UserId)?.Value;

        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            throw new InvalidOperationException("Invalid or missing userId in JWT token");

        return new UserData
        {
            UserId = userId,
            Email = email,
            FirstName = "Test",
            LastName = "User",
            Role = role,
            PreferredLanguageCode = SupportedLanguage.en,
            HebrewLevelValue = HebrewLevel.beginner
        };
    }

    /// <summary>
    /// Logs in with a CreateUser model (uses email, password from the model).
    /// </summary>
    protected async Task<UserData> LoginAsync(CreateUser user)
    {
        var role = Enum.TryParse<Role>(user.Role, true, out var r) ? r : Role.Student;
        
        var loginReq = new LoginRequest { Email = user.Email, Password = user.Password };
        var loginRes = await Client.PostAsJsonAsync(AuthRoutes.Login, loginReq);
        loginRes.EnsureSuccessStatusCode();

        var body = await loginRes.Content.ReadAsStringAsync();
        var tokenRes = JsonSerializer.Deserialize<AccessTokenResponse>(body)
                       ?? throw new InvalidOperationException("Invalid login response");

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenRes.AccessToken);

        // Extract UserId from JWT token to ensure consistency
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenRes.AccessToken);
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == TestConstants.UserId)?.Value;

        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var jwtUserId))
            throw new InvalidOperationException("Invalid or missing userId in JWT token");

        return new UserData
        {
            UserId = jwtUserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = role,
            PreferredLanguageCode = SupportedLanguage.en,
            HebrewLevelValue = HebrewLevel.beginner
        };
    }

    /// <summary>
    /// Assigns a student to a teacher.
    /// </summary>
    protected async Task AssignStudentToTeacherAsync(Guid teacherId, Guid studentId)
    {
        var response = await Client.PostAsync(MappingRoutes.Assign(teacherId, studentId), null);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Sets up a teacher-student relationship and logs in as the teacher.
    /// Returns the teacher and student user data.
    /// </summary>
    protected async Task<(UserData teacher, UserData student)> SetupTeacherStudentRelationshipAsync()
    {
        // Login as Admin to set up teacher-student relationship
        var admin = await CreateUserAsync(role: "admin");
        
        // Create teacher and student
        var teacherModel = await CreateUserViaApiAsync(role: "teacher");
        var studentModel = await CreateUserViaApiAsync(role: "student");
        
        // Assign student to teacher
        await AssignStudentToTeacherAsync(teacherModel.UserId, studentModel.UserId);
        
        // Now login as teacher
        var teacher = await LoginAsync(teacherModel);
        
        var student = new UserData
        {
            UserId = studentModel.UserId,
            Email = studentModel.Email,
            FirstName = studentModel.FirstName,
            LastName = studentModel.LastName,
            Role = Role.Student,
            PreferredLanguageCode = SupportedLanguage.en,
            HebrewLevelValue = HebrewLevel.beginner
        };

        return (teacher, student);
    }

    /// <summary>
    /// Ensures SignalR is connected with the current authenticated user's token.
    /// </summary>
    protected async Task EnsureSignalRConnectedAsync()
    {
        // Extract the current bearer token from HttpClient
        var authHeader = Client.DefaultRequestHeaders.Authorization;
        if (authHeader == null || string.IsNullOrEmpty(authHeader.Parameter))
        {
            throw new InvalidOperationException("No authentication token found. Ensure user is logged in before using SignalR.");
        }

        // Set the token for SignalR and start the connection
        SignalRFixture.UseAccessToken(authHeader.Parameter);
        
        try
        {
            await SignalRFixture.StartAsync();
            OutputHelper.WriteLine($"SignalR connection started successfully with authenticated token");
        }
        catch (Exception ex)
        {
            OutputHelper.WriteLine($"Failed to start SignalR connection: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Generates split sentences for word order game and waits for SignalR event.
    /// Returns the list of generated sentences with attempt IDs.
    /// </summary>
    protected async Task<List<AttemptedSentenceResult>> GenerateSplitSentencesAsync(
        Guid userId,
        Difficulty difficulty = Difficulty.easy,
        bool nikud = false,
        int count = 1,
        TimeSpan? timeout = null)
    {
        await EnsureSignalRConnectedAsync();
        
        SignalRFixture.ClearReceivedMessages();

        var sentenceRequest = new SentenceRequest
        {
            UserId = userId,
            Difficulty = difficulty,
            Nikud = nikud,
            Count = count
        };
        OutputHelper.WriteLine($"Generating sentences for userId: {userId}");
        var response = await PostAsJsonAsync(ApiRoutes.SplitSentences, sentenceRequest);
        response.EnsureSuccessStatusCode();

        var sentenceEvent = await WaitForEventAsync(
                n => n.EventType == EventType.SplitSentenceGeneration,
                timeout ?? TimeSpan.FromSeconds(30)
            );

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var sentences = JsonSerializer.Deserialize<List<AttemptedSentenceResult>>(
            sentenceEvent.Event.Payload.GetRawText(), options)
            ?? throw new InvalidOperationException("Failed to deserialize sentence generation event");

        return sentences;
    }

    /// <summary>
    /// Submits a game attempt with the given answer.
    /// </summary>
    protected async Task<SubmitAttemptResult> SubmitGameAttemptAsync(
        Guid studentId,
        Guid attemptId,
        List<string> givenAnswer)
    {
        var request = new SubmitAttemptRequest
        {
            StudentId = studentId,
            AttemptId = attemptId,
            GivenAnswer = givenAnswer
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.GameAttempt, request);
        response.EnsureSuccessStatusCode();

        return await ReadAsJsonAsync<SubmitAttemptResult>(response)
               ?? throw new InvalidOperationException("Failed to deserialize SubmitAttemptResult");
    }

    /// <summary>
    /// Creates a wrong answer by reversing or shuffling the correct answer.
    /// </summary>
    protected List<string> CreateWrongAnswer(List<string> correctAnswer)
    {
        var wrongAnswer = correctAnswer.ToList();
        
        if (wrongAnswer.Count > 1)
        {
            // Reverse the order to make it wrong
            wrongAnswer.Reverse();
        }
        else
        {
            // If only one word, add a fake word
            wrongAnswer.Add("שגוי");
        }

        return wrongAnswer;
    }

    /// <summary>
    /// Creates a mistake by generating a sentence and submitting a wrong answer.
    /// Returns the attempt result.
    /// </summary>
    protected async Task<SubmitAttemptResult> CreateMistakeAsync(
        Guid studentId,
        Difficulty difficulty = Difficulty.easy,
        bool nikud = false)
    {
        var sentences = await GenerateSplitSentencesAsync(studentId, difficulty, nikud, count: 1);
        var sentence = sentences.First();

        var wrongAnswer = CreateWrongAnswer(sentence.Words);
        var result = await SubmitGameAttemptAsync(studentId, sentence.AttemptId, wrongAnswer);

        return result;
    }

    /// <summary>
    /// Creates a successful attempt by generating a sentence and submitting the correct answer.
    /// Returns the attempt result.
    /// </summary>
    protected async Task<SubmitAttemptResult> CreateSuccessfulAttemptAsync(
        Guid studentId,
        Difficulty difficulty = Difficulty.easy,
        bool nikud = false)
    {
        var sentences = await GenerateSplitSentencesAsync(studentId, difficulty, nikud, count: 1);
        var sentence = sentences.First();

        var result = await SubmitGameAttemptAsync(studentId, sentence.AttemptId, sentence.Words);

        return result;
    }

    /// <summary>
    /// Creates multiple mistakes for a student at different difficulty levels.
    /// </summary>
    protected async Task CreateMultipleMistakesAsync(Guid studentId, int count = 3)
    {
        var difficulties = new[] { Difficulty.easy, Difficulty.medium, Difficulty.hard };
        
        for (int i = 0; i < count; i++)
        {
            var difficulty = difficulties[i % difficulties.Length];
            await CreateMistakeAsync(studentId, difficulty);
        }
    }

    /// <summary>
    /// Tests that mistakes are filtered out when the same sentence is answered correctly later.
    /// Submits the same sentence twice: wrong answer first, then correct answer.
    /// </summary>
    protected async Task<(AttemptedSentenceResult sentence, SubmitAttemptResult failureResult, SubmitAttemptResult successResult)> 
        CreateMistakeWithLaterSuccessAsync(Guid studentId, Difficulty difficulty = Difficulty.easy)
    {
        // Generate one sentence
        var sentences = await GenerateSplitSentencesAsync(studentId, difficulty, nikud: false, count: 1);
        var sentence = sentences.First();

        var wrongAnswer = CreateWrongAnswer(sentence.Words);
        var failureResult = await SubmitGameAttemptAsync(studentId, sentence.AttemptId, wrongAnswer);

        var successResult = await SubmitGameAttemptAsync(studentId, sentence.AttemptId, sentence.Words);

        return (sentence, failureResult, successResult);
    }
}
