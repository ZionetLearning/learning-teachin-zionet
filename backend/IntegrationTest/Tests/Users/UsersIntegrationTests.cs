using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using Manager.Models.Users;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Users;

/// <summary>
/// Users integration tests using per-test user isolation.
/// </summary>
[Collection("Per-test user collection")]
public class UsersIntegrationTests(
    PerTestUserFixture perUserFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : UsersTestBase(perUserFixture, outputHelper, signalRFixture), IAsyncLifetime
{
    [Fact(DisplayName = "POST /users-manager/user - Duplicate email should return 409 Conflict")]
    public async Task CreateUser_DuplicateEmail_Should_Return_Conflict()
    {
        var email = $"dup_{Guid.NewGuid()}@test.com";
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
        var user = TestDataHelper.CreateUser(role: "student");
        var request = BuildRequest(ApiRoutes.User, user, "en-US");
        var response = await Client.SendAsync(request);

        response.ShouldBeCreated();
        var created = await ReadAsJsonAsync<UserData>(response);

        created!.PreferredLanguageCode.Should().Be(SupportedLanguage.en);
        created.HebrewLevelValue.Should().Be(HebrewLevel.beginner);
    }

    [Fact(DisplayName = "POST /users-manager/user - Create user success (he)")]
    public async Task CreateUser_Success_He()
    {
        var user = TestDataHelper.CreateUser(role: "student");
        var request = BuildRequest(ApiRoutes.User, user, "he-IL");
        var response = await Client.SendAsync(request);

        response.ShouldBeCreated();
        var created = await ReadAsJsonAsync<UserData>(response);

        created!.PreferredLanguageCode.Should().Be(SupportedLanguage.he);
        created.HebrewLevelValue.Should().Be(HebrewLevel.beginner);
    }

    [Fact(DisplayName = "POST /users-manager/user - Fallback on unsupported language")]
    public async Task CreateUser_Fallback_UnsupportedLang()
    {
        var user = TestDataHelper.CreateUser();
        var request = BuildRequest(ApiRoutes.User, user, "xx-YY");

        var response = await Client.SendAsync(request);

        response.ShouldBeCreated();
        var created = await ReadAsJsonAsync<UserData>(response);

        created!.PreferredLanguageCode.Should().Be(SupportedLanguage.en);
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
        var user = await CreateUserAsync();
        var response = await Client.GetAsync(ApiRoutes.UserById(user.UserId));

        response.ShouldBeOk();
        var fetched = await ReadAsJsonAsync<UserData>(response);

        fetched!.UserId.Should().Be(user.UserId);
        fetched.Email.Should().Be(user.Email);
    }

    [Fact(DisplayName = "GET /users-manager/user/{id} - With invalid ID should return 404")]
    public async Task GetUser_By_Invalid_Id_Should_Return_NotFound()
    {
        // Create & login a user (to attach token)
        var anyUser = await CreateUserAsync();

        var invalidId = Guid.NewGuid();
        var response = await Client.GetAsync(ApiRoutes.UserById(invalidId));

        response.ShouldBeNotFound();
    }

    [Fact(DisplayName = "PUT /users-manager/user/{id} - Update language valid")]
    public async Task UpdateUser_Language_Valid()
    {
        var user = await CreateUserAsync();
        var update = new UpdateUserModel { PreferredLanguageCode = SupportedLanguage.he };

        var response = await Client.PutAsJsonAsync(ApiRoutes.UserById(user.UserId), update);

        response.ShouldBeOk();
    }

    [Fact(DisplayName = "PUT /users-manager/user/{id} - Invalid language should return 400")]
    public async Task UpdateUser_Language_Invalid()
    {
        var user = await CreateUserAsync();
        var update = new { PreferredLanguageCode = "ru" };

        var response = await Client.PutAsJsonAsync(ApiRoutes.UserById(user.UserId), update);

        response.ShouldBeBadRequest();
    }

    [Fact(DisplayName = "PUT /users-manager/user/{id} - Update HebrewLevel (student)")]
    public async Task UpdateUser_HebrewLevel_Student()
    {
        var user = await CreateUserAsync(role: "student");
        var update = new UpdateUserModel { HebrewLevelValue = HebrewLevel.advanced };

        var response = await Client.PutAsJsonAsync(ApiRoutes.UserById(user.UserId), update);

        response.ShouldBeOk();
    }

    [Fact(DisplayName = "PUT /users-manager/user/{id} - Invalid HebrewLevel should return 400")]
    public async Task UpdateUser_HebrewLevel_Invalid()
    {
        var user = await CreateUserAsync(role: "student");
        var update = new { HebrewLevelValue = "invalid" };

        var response = await Client.PutAsJsonAsync(ApiRoutes.UserById(user.UserId), update);

        response.ShouldBeBadRequest();
    }

    [Fact(DisplayName = "PUT /users-manager/user/{id} - Non-student cannot set HebrewLevel")]
    public async Task UpdateUser_HebrewLevel_NonStudent()
    {
        var user = await CreateUserAsync(role: "teacher");
        var update = new UpdateUserModel { HebrewLevelValue = HebrewLevel.fluent };

        var response = await Client.PutAsJsonAsync(ApiRoutes.UserById(user.UserId), update);

        response.ShouldBeBadRequest();
    }

    [Fact(DisplayName = "DELETE /users-manager/user/{id} - With valid ID should delete user")]
    public async Task DeleteUser_Valid()
    {
        var user = await CreateUserAsync();

        var deleteResponse = await Client.DeleteAsync(ApiRoutes.UserById(user.UserId));
        deleteResponse.ShouldBeOk();

        var getResponse = await Client.GetAsync(ApiRoutes.UserById(user.UserId));
        getResponse.ShouldBeNotFound();
    }

   [Fact(DisplayName = "GET /users-manager/user-list - Should return all users")]
    public async Task GetAllUsers_Should_Return_List()
    {
        // Log in as Admin first
        var admin = await CreateUserAsync(role: "admin");

        // Create two extra users directly via POST (don’t switch auth)
        var u1 = TestDataHelper.CreateUser(email: $"list1_{Guid.NewGuid():N}@test.com");
        var u2 = TestDataHelper.CreateUser(email: $"list2_{Guid.NewGuid():N}@test.com");

        var r1 = await Client.PostAsJsonAsync(ApiRoutes.User, u1);
        r1.ShouldBeCreated();

        var r2 = await Client.PostAsJsonAsync(ApiRoutes.User, u2);
        r2.ShouldBeCreated();

        // Still authenticated as Admin here
        var response = await Client.GetAsync(ApiRoutes.UserList);
        response.ShouldBeOk();

        var users = await ReadAsJsonAsync<List<UserData>>(response);
        users!.Should().Contain(u => u.Email == u1.Email);
        users.Should().Contain(u => u.Email == u2.Email);
    }

    [Fact(DisplayName = "PUT /users-manager/user/{id} - admin user updates student role")]
    public async Task UpdateUser_RoleChange_ByLoggedInAdmin_ShouldSucceed()
    {
        // Log in as Admin 
        var admin = await CreateUserAsync(role: "admin");

        // Create student
        var student = TestDataHelper.CreateUser(email: $"list1_{Guid.NewGuid():N}@test.com");
        var registerStudent = await Client.PostAsJsonAsync(ApiRoutes.User, student);
        registerStudent.ShouldBeCreated();

        // Update student role to teacher
        var update = new UpdateUserModel
        {
            Role = Role.Teacher
        };

        var updateResponse = await Client.PutAsJsonAsync(ApiRoutes.UserById(student.UserId), update);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Confirm the role change
        var getResponse = await Client.GetAsync(ApiRoutes.UserById(student.UserId));
        var updatedStudent = await ReadAsJsonAsync<UserData>(getResponse);
        updatedStudent!.Role.Should().Be(Role.Teacher);
    }

    [Fact(DisplayName = "PUT /users-manager/user/{id} - Student can update interests")]
    public async Task Student_Can_Update_Interests()
    {
        var user = await CreateUserAsync(role: "student");

        var update = new UpdateUserModel
        {
            Interests = ["music", "chess", "travel"]
        };

        var response = await Client.PutAsJsonAsync(ApiRoutes.UserById(user.UserId), update);
        response.ShouldBeOk();

        var updatedUser = await Client.GetAsync(ApiRoutes.UserById(user.UserId));
        updatedUser.ShouldBeOk();

        var userData = await updatedUser.Content.ReadFromJsonAsync<UserData>();
        userData.Should().NotBeNull();
        userData.Interests.Should().NotBeNullOrEmpty();
        userData.Interests.Should().BeEquivalentTo(["music", "chess", "travel"]);
    }

    [Theory(DisplayName = "PUT /users-manager/user/{id} - Non-students cannot update interests")]
    [InlineData("teacher")]
    [InlineData("admin")]
    public async Task NonStudent_Cannot_Set_Interests(string role)
    {
        var user = await CreateUserAsync(role: role);

        var update = new UpdateUserModel
        {
            Interests = ["soccer", "cooking"]
        };

        var response = await Client.PutAsJsonAsync(ApiRoutes.UserById(user.UserId), update);
        response.ShouldBeBadRequest();

        var updatedUser = await Client.GetAsync(ApiRoutes.UserById(user.UserId));

        var userData = await updatedUser.Content.ReadFromJsonAsync<UserData>();
        userData.Should().NotBeNull();
        userData.Interests.Should().BeNullOrEmpty("Non-students should not have interests");
    }


}