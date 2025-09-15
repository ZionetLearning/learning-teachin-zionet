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
    private readonly TestUserHelper _users;

    public TeacherStudentIntegrationTests(SharedTestFixture sharedFixture, ITestOutputHelper output)
    {
        _shared = sharedFixture;
        _client = sharedFixture.HttpFixture.Client;
        _output = output;
        _users = new TestUserHelper(_client);
    }

    public async Task InitializeAsync()
    {
        await _shared.GetAuthenticatedTokenAsync(attachToHttpClient: true);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact(DisplayName = "Admin can assign & unassign any student to any teacher")]
    public async Task Admin_Assign_Unassign_Flow()
    {
        // Create teacher & student
        var teacherId = await _users.CreateUserAsync(Role.Teacher);
        var studentId = await _users.CreateUserAsync(Role.Student);

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

        await _users.CleanupUser(studentId);
        await _users.CleanupUser(teacherId);
    }

    [Fact(DisplayName = "Teacher can assign/unassign only to self; forbidden for other teachers")]
    public async Task Teacher_Can_Only_Manage_Self()
    {
        var (t1Id, t1Email, t1Pwd) = await _users.CreateAndLogin(Role.Teacher);
        var t2Id = await _users.CreateUserAsync(Role.Teacher);
        var s1Id = await _users.CreateUserAsync(Role.Student);

        var t1Token = await _users.LoginAndGetTokenAsync(t1Email, t1Pwd);
        _users.UseBearer(t1Token);

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
        _users.UseBearer(adminToken);

        await _users.CleanupUser(s1Id);
        await _users.CleanupUser(t2Id);
        await _users.CleanupUser(t1Id);
    }

    [Fact(DisplayName = "Student cannot access mapping endpoints")]
    public async Task Student_Cannot_Access_Mapping_Endpoints()
    {
        // Create a student user and login
        var s = TestUserHelper.NewUser(Role.Student);
        var res = await _client.PostAsJsonAsync(UserRoutes.UserBase, s);
        res.EnsureSuccessStatusCode();
        var sToken = await _users.LoginAndGetTokenAsync(s.Email, s.Password);
        _users.UseBearer(sToken);

        // Create a teacher + another student (using Admin token)
        var adminToken = await _shared.GetAuthenticatedTokenAsync(attachToHttpClient: false);
        _users.UseBearer(adminToken);
        var teacherId = await _users.CreateUserAsync(Role.Teacher);
        var studentId = await _users.CreateUserAsync(Role.Student);

        // Use student token to try calling endpoints
        _users.UseBearer(sToken);

        var r1 = await _client.GetAsync(MappingRoutes.ListStudents(teacherId));
        r1.StatusCode.Should().Be(HttpStatusCode.Forbidden); // policy AdminOrTeacher

        var r2 = await _client.PostAsync(MappingRoutes.Assign(teacherId, studentId), null);
        r2.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var r3 = await _client.DeleteAsync(MappingRoutes.Unassign(teacherId, studentId));
        r3.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var r4 = await _client.GetAsync(MappingRoutes.ListTeachers(studentId));
        r4.StatusCode.Should().Be(HttpStatusCode.Forbidden); // AdminOnly

        // Cleanup with Admin
        _users.UseBearer(adminToken);
        await _users.CleanupUser(studentId);
        await _users.CleanupUser(teacherId);
        await _users.CleanupUser(s.UserId);
    }

    [Fact(DisplayName = "Admin can list teachers for a given student")]
    public async Task Admin_List_Teachers_For_Student()
    {
        // Create teacher + student
        var teacherId = await _users.CreateUserAsync(Role.Teacher);
        var studentId = await _users.CreateUserAsync(Role.Student);

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
        await _users.CleanupUser(studentId);
        await _users.CleanupUser(teacherId);
    }
}
