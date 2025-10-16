using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using Manager.Models.Auth;
using Manager.Models.Users;
using System.Text.Json;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Users;

[Collection("IntegrationTests")]
public class TeacherStudentIntegrationTests(
    HttpClientFixture httpClientFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBase(httpClientFixture, outputHelper, signalRFixture)
{
    public override async Task InitializeAsync()
    {
        // Don't login by default - let tests choose which role to use
        SignalRFixture.ClearReceivedMessages();
    }

    /// <summary>
    /// Creates a user and logs them in.
    /// </summary>
    private async Task<UserData> CreateAndLoginAsync(Role role, string? email = null)
    {
        email ??= $"{role.ToString().ToLower()}-{Guid.NewGuid():N}@example.com";

        var user = new UserModel
        {
            UserId = Guid.NewGuid(),
            Email = email,
            Password = TestDataHelper.DefaultTestPassword,
            FirstName = "Test",
            LastName = "User",
            Role = role
        };

        var createRes = await Client.PostAsJsonAsync(UserRoutes.UserBase, user);
        createRes.EnsureSuccessStatusCode();

        // Clear previous auth before logging in
        Client.DefaultRequestHeaders.Authorization = null;

        // Login
        var loginReq = new LoginRequest { Email = user.Email, Password = TestDataHelper.DefaultTestPassword };
        var loginRes = await Client.PostAsJsonAsync(AuthRoutes.Login, loginReq);
        loginRes.EnsureSuccessStatusCode();

        var body = await loginRes.Content.ReadAsStringAsync();
        var tokenRes = JsonSerializer.Deserialize<Models.Auth.AccessTokenResponse>(body)
                       ?? throw new InvalidOperationException("Invalid login response");

        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenRes.AccessToken);

        return new UserData
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = role,
            PreferredLanguageCode = SupportedLanguage.en,
            HebrewLevelValue = HebrewLevel.beginner
        };
    }

    [Fact(DisplayName = "Admin can assign & unassign any student to any teacher")]
    public async Task Admin_Assign_Unassign_Flow()
    {
        // Login as Admin
        await CreateAndLoginAsync(Role.Admin);

        // Create teacher & student as Admin (no login switch)
        var teacher = TestDataHelper.CreateUser(role: "teacher");
        var student = TestDataHelper.CreateUser(role: "student");

        (await Client.PostAsJsonAsync(UserRoutes.UserBase, teacher)).EnsureSuccessStatusCode();
        (await Client.PostAsJsonAsync(UserRoutes.UserBase, student)).EnsureSuccessStatusCode();

        // Assign
        var assign = await Client.PostAsync(MappingRoutes.Assign(teacher.UserId, student.UserId), null);
        assign.StatusCode.Should().Be(HttpStatusCode.OK);

        // Idempotent assign
        var assign2 = await Client.PostAsync(MappingRoutes.Assign(teacher.UserId, student.UserId), null);
        assign2.StatusCode.Should().Be(HttpStatusCode.OK);

        // List students (admin can list any)
        var list = await Client.GetAsync(MappingRoutes.ListStudents(teacher.UserId));
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        var students = await list.Content.ReadFromJsonAsync<List<UserData>>() ?? new();
        students.Should().Contain(s => s.UserId == student.UserId);

        // Unassign
        var unassign = await Client.DeleteAsync(MappingRoutes.Unassign(teacher.UserId, student.UserId));
        unassign.StatusCode.Should().Be(HttpStatusCode.OK);

        // Idempotent unassign
        var unassign2 = await Client.DeleteAsync(MappingRoutes.Unassign(teacher.UserId, student.UserId));
        unassign2.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "Teacher can assign/unassign only to self; forbidden for other teachers")]
    public async Task Teacher_Can_Only_Manage_Self()
    {
        // Login as Teacher1 (active actor)
        var teacher1 = await CreateAndLoginAsync(Role.Teacher);

        // Create Teacher2 + Student via Teacher1 token
        var teacher2 = TestDataHelper.CreateUser(role: "teacher");
        var student = TestDataHelper.CreateUser(role: "student");

        (await Client.PostAsJsonAsync(UserRoutes.UserBase, teacher2)).EnsureSuccessStatusCode();
        (await Client.PostAsJsonAsync(UserRoutes.UserBase, student)).EnsureSuccessStatusCode();

        // Teacher1 can assign/unassign to self
        var okAssignSelf = await Client.PostAsync(MappingRoutes.Assign(teacher1.UserId, student.UserId), null);
        okAssignSelf.StatusCode.Should().Be(HttpStatusCode.OK);

        var forbidAssignOther = await Client.PostAsync(MappingRoutes.Assign(teacher2.UserId, student.UserId), null);
        forbidAssignOther.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var okUnassignSelf = await Client.DeleteAsync(MappingRoutes.Unassign(teacher1.UserId, student.UserId));
        okUnassignSelf.StatusCode.Should().Be(HttpStatusCode.OK);

        var forbidUnassignOther = await Client.DeleteAsync(MappingRoutes.Unassign(teacher2.UserId, student.UserId));
        forbidUnassignOther.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // List students
        var okListSelf = await Client.GetAsync(MappingRoutes.ListStudents(teacher1.UserId));
        okListSelf.StatusCode.Should().Be(HttpStatusCode.OK);

        var forbidListOther = await Client.GetAsync(MappingRoutes.ListStudents(teacher2.UserId));
        forbidListOther.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Student cannot access mapping endpoints")]
    public async Task Student_Cannot_Access_Mapping_Endpoints()
    {
        // Login as Student (active actor)
        var student = await CreateAndLoginAsync(Role.Student);

        // Create Teacher + another Student via Student's token
        var teacher = TestDataHelper.CreateUser(role: "teacher");
        var anotherStudent = TestDataHelper.CreateUser(role: "student");

        (await Client.PostAsJsonAsync(UserRoutes.UserBase, teacher)).EnsureSuccessStatusCode();
        (await Client.PostAsJsonAsync(UserRoutes.UserBase, anotherStudent)).EnsureSuccessStatusCode();

        // Student tries forbidden endpoints
        var r1 = await Client.GetAsync(MappingRoutes.ListStudents(teacher.UserId));
        r1.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var r2 = await Client.PostAsync(MappingRoutes.Assign(teacher.UserId, anotherStudent.UserId), null);
        r2.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var r3 = await Client.DeleteAsync(MappingRoutes.Unassign(teacher.UserId, anotherStudent.UserId));
        r3.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var r4 = await Client.GetAsync(MappingRoutes.ListTeachers(anotherStudent.UserId));
        r4.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Admin can list teachers for a given student")]
    public async Task Admin_List_Teachers_For_Student()
    {
        // Login as Admin (active actor)
        await CreateAndLoginAsync(Role.Admin);

        // Create Teacher + Student via Admin token
        var teacher = TestDataHelper.CreateUser(role: "teacher");
        var student = TestDataHelper.CreateUser(role: "student");

        (await Client.PostAsJsonAsync(UserRoutes.UserBase, teacher)).EnsureSuccessStatusCode();
        (await Client.PostAsJsonAsync(UserRoutes.UserBase, student)).EnsureSuccessStatusCode();

        // Assign teacher → student
        var assign = await Client.PostAsync(MappingRoutes.Assign(teacher.UserId, student.UserId), null);
        assign.StatusCode.Should().Be(HttpStatusCode.OK);

        // Admin lists teachers for the student
        var list = await Client.GetAsync(MappingRoutes.ListTeachers(student.UserId));
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        var teachers = await list.Content.ReadFromJsonAsync<List<UserData>>() ?? new();
        teachers.Should().Contain(t => t.UserId == teacher.UserId);
    }
}