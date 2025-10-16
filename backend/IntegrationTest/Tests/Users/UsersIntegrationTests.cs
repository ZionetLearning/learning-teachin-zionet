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
/// Users integration tests using HttpClientFixture.
/// </summary>
[Collection("IntegrationTests")]
public class UsersIntegrationTests(
    HttpClientFixture httpClientFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : UsersTestBase(httpClientFixture, outputHelper, signalRFixture)
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
    }

    [Fact(DisplayName = "POST /users-manager/user - Create user success (he)")]
    public async Task CreateUser_Success_He()
    {
        var user = TestDataHelper.CreateUser(role: "student");
        var request = BuildRequest(ApiRoutes.User, user, "he-IL");
        var response = await Client.SendAsync(request);

        response.ShouldBeCreated();
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

        // Create two extra users directly via POST (don't switch auth)
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

    [Fact(DisplayName = "PUT /users-manager/user/interests/{id} - Student can set their own interests")]
    public async Task Student_Can_Set_Their_Own_Interests()
    {
        var student = await CreateUserAsync();

        var update = new UpdateInterestsRequest
        {
            Interests = ["math", "science", "history"]
        };

        var response = await Client.PutAsJsonAsync(ApiRoutes.UserSetInterests(student.UserId), update);
        response.ShouldBeOk();

        var fetched = await Client.GetAsync(ApiRoutes.UserById(student.UserId));
        fetched.ShouldBeOk();

        var data = await ReadAsJsonAsync<UserData>(fetched);
        data!.Interests.Should().BeEquivalentTo(update.Interests);
    }

    [Fact(DisplayName = "PUT /users-manager/user/interests/{id} - Admin can update student interests")]
    public async Task Admin_Can_Set_Interests_For_Any_Student()
    {
        var admin = await CreateUserAsync(role: "admin");
        var student = TestDataHelper.CreateUser(role: "student");

        var registerStudent = await Client.PostAsJsonAsync(ApiRoutes.User, student);
        registerStudent.ShouldBeCreated();

        // Still authenticated as admin here
        var update = new UpdateInterestsRequest
        {
            Interests = ["biology", "coding"]
        };

        var response = await Client.PutAsJsonAsync(ApiRoutes.UserSetInterests(student.UserId), update);
        response.ShouldBeOk();

        var fetched = await Client.GetAsync(ApiRoutes.UserById(student.UserId));
        fetched.ShouldBeOk();
        var data = await ReadAsJsonAsync<UserData>(fetched);
        data!.Interests.Should().BeEquivalentTo(update.Interests);
    }

    [Fact(DisplayName = "PUT /users-manager/user/interests/{id} - Student cannot set another student's interests")]
    public async Task Student_Cannot_Update_Other_Student_Interests()
    {
        var _ = await CreateUserAsync();
        var student2 = TestDataHelper.CreateUser(role: "student");

        var registerOther = await Client.PostAsJsonAsync(ApiRoutes.User, student2);
        registerOther.ShouldBeCreated();

        var update = new UpdateInterestsRequest
        {
            Interests = ["football", "gaming"]
        };

        var response = await Client.PutAsJsonAsync(ApiRoutes.UserSetInterests(student2.UserId), update);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}