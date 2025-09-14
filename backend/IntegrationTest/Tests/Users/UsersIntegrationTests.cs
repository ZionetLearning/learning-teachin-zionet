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
) : UsersTestBase(sharedFixture, outputHelper, signalRFixture)
{
    [Fact(DisplayName = "POST /users-manager/user - Duplicate email should return 409 Conflict")]
    public async Task CreateUser_DuplicateEmail_Should_Return_Conflict()
    {
        var email = $"dup_{Guid.NewGuid()}@test.com";

        // login not needed here, just create 2 users
        var user1 = TestDataHelper.CreateUserWithFixedEmail(email);
        var user2 = TestDataHelper.CreateUserWithFixedEmail(email);

        var r1 = await Client.PostAsJsonAsync(ApiRoutes.User, user1);
        r1.ShouldBeCreated();

        var r2 = await Client.PostAsJsonAsync(ApiRoutes.User, user2);
        r2.ShouldBeConflict();
    }

    [Fact(DisplayName = "POST /users-manager/user - Create user success (en)")]
    public async Task CreateUser_Success_En()
    {
        var user = await CreateUserAsync("student", acceptLanguage: "en-US");
        OutputHelper.WriteLine($"Created user with email {user.Email}");

        var fetched = await Client.GetFromJsonAsync<UserData>(ApiRoutes.UserById(user.UserId));
        fetched!.PreferredLanguageCode.Should().Be(SupportedLanguage.en);
    }

    [Fact(DisplayName = "POST /users-manager/user - Create user success (he)")]
    public async Task CreateUser_Success_He()
    {
        var user = await CreateUserAsync("student", acceptLanguage: "he-IL");
        OutputHelper.WriteLine($"Created user with email {user.Email}");

        var fetched = await Client.GetFromJsonAsync<UserData>(ApiRoutes.UserById(user.UserId));
        fetched!.PreferredLanguageCode.Should().Be(SupportedLanguage.he);
    }

    [Fact(DisplayName = "POST /users-manager/user - Invalid role should return 400")]
    public async Task CreateUser_InvalidRole()
    {
        var user = TestDataHelper.CreateUser(role: "alien");
        var response = await Client.PostAsJsonAsync(ApiRoutes.User, user);
        response.ShouldBeBadRequest();
    }

    [Fact(DisplayName = "GET /users-manager/user/{id} - With valid ID should return user")]
    public async Task GetUser_By_Valid_Id_Should_Return_User()
    {
        var (user, _) = await CreateAndLoginUserAsync();
        var response = await Client.GetAsync(ApiRoutes.UserById(user.UserId));

        response.ShouldBeOk();
        var fetched = await ReadAsJsonAsync<UserData>(response);
        fetched!.UserId.Should().Be(user.UserId);
    }

    [Fact(DisplayName = "GET /users-manager/user/{id} - With invalid ID should return 404")]
    public async Task GetUser_By_Invalid_Id_Should_Return_NotFound()
    {
        var response = await Client.GetAsync(ApiRoutes.UserById(Guid.NewGuid()));
        response.ShouldBeNotFound();
    }

    [Fact(DisplayName = "PUT /users-manager/user/{id} - Update language valid")]
    public async Task UpdateUser_Language_Valid()
    {
        var (user, _) = await CreateAndLoginUserAsync();
        var update = new UpdateUserModel { PreferredLanguageCode = SupportedLanguage.he };

        var response = await Client.PutAsJsonAsync(ApiRoutes.UserById(user.UserId), update);
        response.ShouldBeOk();
    }

    [Fact(DisplayName = "PUT /users-manager/user/{id} - Non-student cannot set HebrewLevel")]
    public async Task UpdateUser_HebrewLevel_NonStudent()
    {
        var (teacher, _) = await CreateAndLoginUserAsync("teacher");
        var update = new UpdateUserModel { HebrewLevelValue = HebrewLevel.fluent };

        var response = await Client.PutAsJsonAsync(ApiRoutes.UserById(teacher.UserId), update);
        response.ShouldBeBadRequest();
    }

    [Fact(DisplayName = "DELETE /users-manager/user/{id} - With valid ID should delete user")]
    public async Task DeleteUser_Valid()
    {
        var (user, _) = await CreateAndLoginUserAsync();
        var deleteResponse = await Client.DeleteAsync(ApiRoutes.UserById(user.UserId));

        deleteResponse.ShouldBeOk();
        var getResponse = await Client.GetAsync(ApiRoutes.UserById(user.UserId));
        getResponse.ShouldBeNotFound();
    }

    [Fact(DisplayName = "GET /users-manager/user-list - Should return all users")]
    public async Task GetAllUsers_Should_Return_List()
    {
        var (u1, _) = await CreateAndLoginUserAsync(email: $"list1_{Guid.NewGuid():N}@test.com");
        var (u2, _) = await CreateAndLoginUserAsync(email: $"list2_{Guid.NewGuid():N}@test.com");

        var response = await Client.GetAsync(ApiRoutes.UserList);
        response.ShouldBeOk();

        var users = await ReadAsJsonAsync<List<UserData>>(response);
        users!.Should().Contain(u => u.Email == u1.Email);
        users.Should().Contain(u => u.Email == u2.Email);
    }
}