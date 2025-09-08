using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using Manager.Models.Users;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Users;

[Collection("Shared test collection")]
public class UsersIntegrationTests(
    SharedTestFixture sharedFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : UsersTestBase(sharedFixture, outputHelper, signalRFixture), IAsyncLifetime
{

    [Fact(DisplayName = "POST /users-manager/user - Duplicate email should return 409 Conflict")]
    public async Task CreateUser_DuplicateEmail_Should_Return_Conflict()
    {
        var user1 = TestDataHelper.CreateUserWithFixedEmail();
        var user2 = TestDataHelper.CreateUserWithFixedEmail(); // same email, different Guid

        // First create succeeds
        var r1 = await Client.PostAsJsonAsync(ApiRoutes.User, user1);
        r1.ShouldBeCreated();

        // Second should fail
        var r2 = await Client.PostAsJsonAsync(ApiRoutes.User, user2);
        r2.ShouldBeConflict();
    }

    [Fact(DisplayName = "POST /users-manager/user - Create user success (en)")]
    public async Task CreateUser_Success_En()
    {
        OutputHelper.WriteLine("Running: CreateUser_Success_En");

        var user = TestDataHelper.CreateUser(role: "student");
        OutputHelper.WriteLine($"Creating user with email: {user.Email}");

        var request = BuildRequest(ApiRoutes.User, user, "en-US");
        var response = await Client.SendAsync(request);

        response.ShouldBeCreated();
        OutputHelper.WriteLine("User created successfully with Accept-Language: en-US");

        var created = await ReadAsJsonAsync<UserData>(response);
        created!.PreferredLanguageCode.Should().Be(SupportedLanguage.en);
        created.HebrewLevelValue.Should().Be(HebrewLevel.beginner);
    }

    [Fact(DisplayName = "POST /users-manager/user - Create user success (he)")]
    public async Task CreateUser_Success_He()
    {
        OutputHelper.WriteLine("Running: CreateUser_Success_He");

        var user = TestDataHelper.CreateUser(role: "student");
        OutputHelper.WriteLine($"Creating user with email: {user.Email}");

        var request = BuildRequest(ApiRoutes.User, user, "he-IL");
        var response = await Client.SendAsync(request);

        response.ShouldBeCreated();
        OutputHelper.WriteLine("User created successfully with Accept-Language: he-IL");

        var created = await ReadAsJsonAsync<UserData>(response);
        created!.PreferredLanguageCode.Should().Be(SupportedLanguage.he);
        created.HebrewLevelValue.Should().Be(HebrewLevel.beginner);
    }

    [Fact(DisplayName = "POST /users-manager/user - Fallback on unsupported language")]
    public async Task CreateUser_Fallback_UnsupportedLang()
    {
        OutputHelper.WriteLine("Running: CreateUser_Fallback_UnsupportedLang");

        var user = TestDataHelper.CreateUser();
        var request = BuildRequest(ApiRoutes.User, user, "xx-YY");

        var response = await Client.SendAsync(request);

        response.ShouldBeCreated();
        OutputHelper.WriteLine("User created with unsupported language, expecting fallback to 'en'");

        var created = await ReadAsJsonAsync<UserData>(response);
        created!.PreferredLanguageCode.Should().Be(SupportedLanguage.en);
    }

    [Fact(DisplayName = "POST /users-manager/user - Invalid role should return 400")]
    public async Task CreateUser_InvalidRole()
    {
        OutputHelper.WriteLine("Running: CreateUser_InvalidRole");

        var user = TestDataHelper.CreateUser(role: "alien");
        var response = await Client.PostAsJsonAsync(ApiRoutes.User, user);

        response.ShouldBeBadRequest();
        OutputHelper.WriteLine("Invalid role correctly returned 400 BadRequest");
    }

    [Fact(DisplayName = "GET /users-manager/user/{id} - With valid ID should return user")]
    public async Task GetUser_By_Valid_Id_Should_Return_User()
    {
        var user = await CreateUserAsync();
        OutputHelper.WriteLine($"Created user with ID: {user.UserId}");

        var response = await Client.GetAsync(ApiRoutes.UserById(user.UserId));
        response.ShouldBeOk();

        var fetched = await ReadAsJsonAsync<UserData>(response);
        fetched!.UserId.Should().Be(user.UserId);
        fetched.Email.Should().Be(user.Email);

        OutputHelper.WriteLine($"Fetched user matches created: ID={fetched.UserId}, Email={fetched.Email}");
    }

    [Fact(DisplayName = "GET /users-manager/user/{id} - With invalid ID should return 404")]
    public async Task GetUser_By_Invalid_Id_Should_Return_NotFound()
    {
        var invalidId = Guid.NewGuid();
        OutputHelper.WriteLine($"Running: GetUser_By_Invalid_Id with ID {invalidId}");

        var response = await Client.GetAsync(ApiRoutes.UserById(invalidId));
        response.ShouldBeNotFound();
    }

    [Fact(DisplayName = "PUT /users-manager/user/{id} - Update language valid")]
    public async Task UpdateUser_Language_Valid()
    {
        var user = await CreateUserAsync();
        OutputHelper.WriteLine($"Updating language for user {user.UserId}");

        var update = new UpdateUserModel { PreferredLanguageCode = SupportedLanguage.he };
        var response = await Client.PutAsJsonAsync(ApiRoutes.UserById(user.UserId), update);

        response.ShouldBeOk();
        OutputHelper.WriteLine("Language updated successfully");
    }

    [Fact(DisplayName = "PUT /users-manager/user/{id} - Invalid language should return 400")]
    public async Task UpdateUser_Language_Invalid()
    {
        var user = await CreateUserAsync();
        OutputHelper.WriteLine($"Updating with invalid language for user {user.UserId}");

        var update = new { PreferredLanguageCode = "ru" }; // unsupported
        var response = await Client.PutAsJsonAsync(ApiRoutes.UserById(user.UserId), update);

        response.ShouldBeBadRequest();
        OutputHelper.WriteLine("Invalid language correctly returned 400 BadRequest");
    }

    [Fact(DisplayName = "PUT /users-manager/user/{id} - Update HebrewLevel (student)")]
    public async Task UpdateUser_HebrewLevel_Student()
    {
        var user = await CreateUserAsync(role: "student");
        OutputHelper.WriteLine($"Updating HebrewLevel for student {user.UserId}");

        var update = new UpdateUserModel { HebrewLevelValue = HebrewLevel.advanced };
        var response = await Client.PutAsJsonAsync(ApiRoutes.UserById(user.UserId), update);

        response.ShouldBeOk();
        OutputHelper.WriteLine("HebrewLevel updated successfully for student");
    }

    [Fact(DisplayName = "PUT /users-manager/user/{id} - Invalid HebrewLevel should return 400")]
    public async Task UpdateUser_HebrewLevel_Invalid()
    {
        var user = await CreateUserAsync(role: "student");
        OutputHelper.WriteLine($"Updating invalid HebrewLevel for student {user.UserId}");

        var update = new { HebrewLevelValue = "invalid" };
        var response = await Client.PutAsJsonAsync(ApiRoutes.UserById(user.UserId), update);

        response.ShouldBeBadRequest();
        OutputHelper.WriteLine("Invalid HebrewLevel correctly returned 400 BadRequest");
    }

    [Fact(DisplayName = "PUT /users-manager/user/{id} - Non-student cannot set HebrewLevel")]
    public async Task UpdateUser_HebrewLevel_NonStudent()
    {
        var user = await CreateUserAsync(role: "teacher");
        OutputHelper.WriteLine($"Trying to set HebrewLevel for non-student {user.UserId}");

        var update = new UpdateUserModel { HebrewLevelValue = HebrewLevel.fluent };
        var response = await Client.PutAsJsonAsync(ApiRoutes.UserById(user.UserId), update);

        response.ShouldBeBadRequest();
        OutputHelper.WriteLine("Non-student HebrewLevel update correctly returned 400 BadRequest");
    }

    [Fact(DisplayName = "DELETE /users-manager/user/{id} - With valid ID should delete user")]
    public async Task DeleteUser_Valid()
    {
        var user = await CreateUserAsync();
        OutputHelper.WriteLine($"Deleting user {user.UserId}");

        var deleteResponse = await Client.DeleteAsync(ApiRoutes.UserById(user.UserId));
        deleteResponse.ShouldBeOk();

        var getResponse = await Client.GetAsync(ApiRoutes.UserById(user.UserId));
        getResponse.ShouldBeNotFound();

        OutputHelper.WriteLine("User deleted and confirmed not found");
    }

    [Fact(DisplayName = "GET /users-manager/user-list - Should return all users")]
    public async Task GetAllUsers_Should_Return_List()
    {
        var u1 = await CreateUserAsync(email: $"list1_{Guid.NewGuid():N}@test.com");
        var u2 = await CreateUserAsync(email: $"list2_{Guid.NewGuid():N}@test.com");
        OutputHelper.WriteLine($"Created 2 users: {u1.Email}, {u2.Email}");

        var response = await Client.GetAsync(ApiRoutes.UserList);
        response.ShouldBeOk();

        var users = await ReadAsJsonAsync<List<UserData>>(response);
        users!.Should().Contain(u => u.Email == u1.Email);
        users.Should().Contain(u => u.Email == u2.Email);

        OutputHelper.WriteLine($"Verified users count: {users.Count}");
    }
}