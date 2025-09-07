using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Models.Auth;
using Manager.Models.Auth;
using Manager.Models.Users;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Users;

[Collection("Shared test collection")]
public class TeacherStudentIntegrationTests : IAsyncLifetime
{
    private readonly SharedTestFixture _shared;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public TeacherStudentIntegrationTests(SharedTestFixture sharedFixture, ITestOutputHelper output)
    {
        _shared = sharedFixture;
        _client = sharedFixture.HttpFixture.Client;
        _output = output;
    }

    public async Task InitializeAsync()
    {
        await _shared.GetAuthenticatedTokenAsync(attachToHttpClient: true);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static CreateUser NewUser(Role role) => new()
    {
        UserId = Guid.NewGuid(),
        Email = $"{role.ToString().ToLower()}-{Guid.NewGuid():N}@example.com",
        Password = "Test123!",
        FirstName = role.ToString(),
        LastName = "Auto",
        Role = role.ToString()
    };

    private async Task<Guid> CreateUserAsync(Role role)
    {
        var u = NewUser(role);
        var res = await _client.PostAsJsonAsync(UserRoutes.UserBase, u);
        res.EnsureSuccessStatusCode();
        return u.UserId;
    }

    private async Task<string> LoginAndGetTokenAsync(string email, string password)
    {
        var res = await _client.PostAsJsonAsync(AuthRoutes.Login, new LoginRequest { Email = email, Password = password });
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<AccessTokenResponse>(body) ?? throw new InvalidOperationException("Invalid JSON");
        return dto.AccessToken;
    }

    private async Task<(Guid id, string email, string password)> CreateAndLogin(Role role)
    {
        var u = NewUser(role);
        var res = await _client.PostAsJsonAsync(UserRoutes.UserBase, u);
        res.EnsureSuccessStatusCode();
        var token = await LoginAndGetTokenAsync(u.Email, u.Password);
        return (u.UserId, u.Email, u.Password);
    }

    private void UseBearer(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private async Task CleanupUser(Guid id)
    {
        var del = await _client.DeleteAsync(UserRoutes.UserById(id));
        if (del.StatusCode != HttpStatusCode.OK && del.StatusCode != HttpStatusCode.NotFound)
            del.EnsureSuccessStatusCode();
    }

    [Fact(DisplayName = "Admin can assign & unassign any student to any teacher")]
    public async Task Admin_Assign_Unassign_Flow()
    {
        // Create teacher & student
        var teacherId = await CreateUserAsync(Role.Teacher);
        var studentId = await CreateUserAsync(Role.Student);

        // Assign
        var assign = await _client.PostAsync(MappingRoutes.Assign(teacherId, studentId), content: null);
        assign.StatusCode.Should().Be(HttpStatusCode.OK);

        // Idempotent assign
        var assign2 = await _client.PostAsync(MappingRoutes.Assign(teacherId, studentId), content: null);
        assign2.StatusCode.Should().Be(HttpStatusCode.OK);

        // List students (admin can list any)
        var list = await _client.GetAsync(MappingRoutes.ListStudents(teacherId));
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        var students = await list.Content.ReadFromJsonAsync<List<UserData>>() ?? new();
        students.Should().Contain(s => s.UserId == studentId);

        // Unassign
        var unassign = await _client.DeleteAsync(MappingRoutes.Unassign(teacherId, studentId));
        unassign.StatusCode.Should().Be(HttpStatusCode.OK);

        // Idempotent unassign
        var unassign2 = await _client.DeleteAsync(MappingRoutes.Unassign(teacherId, studentId));
        unassign2.StatusCode.Should().Be(HttpStatusCode.OK);

        await CleanupUser(studentId);
        await CleanupUser(teacherId);
    }

    [Fact(DisplayName = "Teacher can assign/unassign only to self; forbidden for other teachers")]
    public async Task Teacher_Can_Only_Manage_Self()
    {
        var (t1Id, t1Email, t1Pwd) = await CreateAndLogin(Role.Teacher);
        var t2Id = await CreateUserAsync(Role.Teacher);
        var s1Id = await CreateUserAsync(Role.Student);

        var t1Token = await LoginAndGetTokenAsync(t1Email, t1Pwd);
        UseBearer(t1Token);

        var okAssignSelf = await _client.PostAsync(MappingRoutes.Assign(t1Id, s1Id), null);
        okAssignSelf.StatusCode.Should().Be(HttpStatusCode.OK);

        var forbidAssignOther = await _client.PostAsync(MappingRoutes.Assign(t2Id, s1Id), null);
        forbidAssignOther.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var okUnassignSelf = await _client.DeleteAsync(MappingRoutes.Unassign(t1Id, s1Id));
        okUnassignSelf.StatusCode.Should().Be(HttpStatusCode.OK);

        var forbidUnassignOther = await _client.DeleteAsync(MappingRoutes.Unassign(t2Id, s1Id));
        forbidUnassignOther.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var okListSelf = await _client.GetAsync(MappingRoutes.ListStudents(t1Id));
        okListSelf.StatusCode.Should().Be(HttpStatusCode.OK);

        var forbidListOther = await _client.GetAsync(MappingRoutes.ListStudents(t2Id));
        forbidListOther.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var adminToken = await _shared.GetAuthenticatedTokenAsync(attachToHttpClient: false);
        UseBearer(adminToken);

        await CleanupUser(s1Id);
        await CleanupUser(t2Id);
        await CleanupUser(t1Id);
    }

    [Fact(DisplayName = "Student cannot access mapping endpoints")]
    public async Task Student_Cannot_Access_Mapping_Endpoints()
    {
        // Create a student user and login
        var s = NewUser(Role.Student);
        var res = await _client.PostAsJsonAsync(UserRoutes.UserBase, s);
        res.EnsureSuccessStatusCode();
        var sToken = await LoginAndGetTokenAsync(s.Email, s.Password);
        UseBearer(sToken);

        // Create a teacher + another student (using Admin token)
        var adminToken = await _shared.GetAuthenticatedTokenAsync(attachToHttpClient: false);
        UseBearer(adminToken);
        var teacherId = await CreateUserAsync(Role.Teacher);
        var studentId = await CreateUserAsync(Role.Student);

        // Use student token to try calling endpoints
        UseBearer(sToken);

        var r1 = await _client.GetAsync(MappingRoutes.ListStudents(teacherId));
        r1.StatusCode.Should().Be(HttpStatusCode.Forbidden); // policy AdminOrTeacher

        var r2 = await _client.PostAsync(MappingRoutes.Assign(teacherId, studentId), null);
        r2.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var r3 = await _client.DeleteAsync(MappingRoutes.Unassign(teacherId, studentId));
        r3.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var r4 = await _client.GetAsync(MappingRoutes.ListTeachers(studentId));
        r4.StatusCode.Should().Be(HttpStatusCode.Forbidden); // AdminOnly

        // Cleanup with Admin
        UseBearer(adminToken);
        await CleanupUser(studentId);
        await CleanupUser(teacherId);
        await CleanupUser(s.UserId);
    }

    [Fact(DisplayName = "Admin can list teachers for a given student")]
    public async Task Admin_List_Teachers_For_Student()
    {
        // Create teacher + student
        var teacherId = await CreateUserAsync(Role.Teacher);
        var studentId = await CreateUserAsync(Role.Student);

        // Assign
        var assign = await _client.PostAsync(MappingRoutes.Assign(teacherId, studentId), null);
        assign.StatusCode.Should().Be(HttpStatusCode.OK);

        // Admin lists teachers for the student
        var list = await _client.GetAsync(MappingRoutes.ListTeachers(studentId));
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        var teachers = await list.Content.ReadFromJsonAsync<List<UserData>>() ?? new();
        teachers.Should().Contain(t => t.UserId == teacherId);

        // Cleanup
        await _client.DeleteAsync(MappingRoutes.Unassign(teacherId, studentId));
        await CleanupUser(studentId);
        await CleanupUser(teacherId);
    }
}
