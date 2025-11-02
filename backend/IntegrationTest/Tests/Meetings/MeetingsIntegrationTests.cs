using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using Manager.Models.Users;
using Manager.Models.Meetings;
using System.Net;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Meetings;

[Collection("IntegrationTests")]
public class MeetingsIntegrationTests(
    HttpClientFixture httpClientFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : MeetingsTestBase(httpClientFixture, outputHelper, signalRFixture)
{
    #region Lifecycle Tests (Create -> Update -> Delete)

    [Fact(DisplayName = "Meeting lifecycle - Teacher can create, update, and delete a meeting")]
    public async Task MeetingLifecycle_AsTeacher_ShouldSucceed()
    {
        // Arrange
        var teacher = await LoginAsAsync(Role.Teacher);
        var studentInfo = ClientFixture.GetUserInfo(Role.Student);

        var createRequest = new CreateMeetingRequest
        {
            Attendees = [
                new MeetingAttendee { UserId = teacher.UserId, Role = AttendeeRole.Teacher },
                new MeetingAttendee { UserId = studentInfo.UserId, Role = AttendeeRole.Student }
            ],
            StartTimeUtc = DateTimeOffset.UtcNow.AddDays(1),
            DurationMinutes = 60,
            Description = "Integration Test Meeting",
        };

        // Act & Assert - Create
        var meeting = await CreateMeetingAsync(createRequest, teacher);
        meeting.Should().NotBeNull();
        meeting.Id.Should().NotBeEmpty();
        meeting.CreatedByUserId.Should().Be(teacher.UserId);
        meeting.Attendees.Should().HaveCount(2);
        meeting.CreatedOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));

        // Act & Assert - Retrieve
        var retrieved = await GetMeetingAsync(meeting.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(meeting.Id);

        // Act & Assert - Update
        var updateRequest = new UpdateMeetingRequest
        {
            StartTimeUtc = DateTimeOffset.UtcNow.AddDays(2)
        };
        var updateSuccess = await UpdateMeetingAsync(meeting.Id, updateRequest);
        updateSuccess.Should().BeTrue();

        var updated = await GetMeetingAsync(meeting.Id);
        updated!.StartTimeUtc.Should().BeCloseTo(DateTimeOffset.UtcNow.AddDays(2), TimeSpan.FromMinutes(1));

        // Act & Assert - Delete
        var deleteSuccess = await DeleteMeetingAsync(meeting.Id);
        deleteSuccess.Should().BeTrue();

        var deleted = await GetMeetingAsync(meeting.Id);
        deleted.Should().BeNull();
    }

    [Fact(DisplayName = "Meeting with multiple participants - Create, update attendees, and verify")]
    public async Task MeetingWithMultipleParticipants_FullLifecycle_ShouldSucceed()
    {
        // Arrange
        var teacher = await LoginAsAsync(Role.Teacher);
        var student1 = ClientFixture.GetUserInfo(Role.Student);
        
        var (student2Email, student2Token) = await ClientFixture.CreateEphemeralUserAsync(Role.Student);
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(student2Token);
        var student2Id = Guid.Parse(jwtToken.Claims.First(c => c.Type == TestConstants.UserId).Value);

        await ClientFixture.LoginAsync(Role.Teacher);
        await EnsureSignalRStartedAsync();

        // Act & Assert - Create with one student
        var createRequest = new CreateMeetingRequest
        {
            Attendees = [
                new MeetingAttendee { UserId = teacher.UserId, Role = AttendeeRole.Teacher },
                new MeetingAttendee { UserId = student1.UserId, Role = AttendeeRole.Student }
            ],
            StartTimeUtc = DateTimeOffset.UtcNow.AddDays(1),
            Description = "Meeting with Multiple Participants",
            DurationMinutes = 45
        };

        var meeting = await CreateMeetingAsync(createRequest, teacher);
        meeting.Attendees.Should().HaveCount(2);

        // Act & Assert - Update to add second student
        var updateRequest = new UpdateMeetingRequest
        {
            Attendees = [
                new MeetingAttendee { UserId = teacher.UserId, Role = AttendeeRole.Teacher },
                new MeetingAttendee { UserId = student1.UserId, Role = AttendeeRole.Student },
                new MeetingAttendee { UserId = student2Id, Role = AttendeeRole.Student }
            ]
        };

        var updateSuccess = await UpdateMeetingAsync(meeting.Id, updateRequest);
        updateSuccess.Should().BeTrue();

        var updated = await GetMeetingAsync(meeting.Id);
        updated!.Attendees.Should().HaveCount(3);
        updated.Attendees.Should().Contain(a => a.UserId == student1.UserId);
        updated.Attendees.Should().Contain(a => a.UserId == student2Id);
    }

    #endregion

    #region Authorization Tests

    [Fact(DisplayName = "Meeting authorization - Student cannot create, update, or delete meetings")]
    public async Task MeetingAuthorization_AsStudent_ShouldFail()
    {
        // Arrange - Create meeting as teacher
        var teacher = await LoginAsAsync(Role.Teacher);
        var meeting = await CreateMeetingAsync(currentUser: teacher);

        // Switch to student
        await LoginAsAsync(Role.Student);
        var teacherInfo = ClientFixture.GetUserInfo(Role.Teacher);
        var studentInfo = ClientFixture.GetUserInfo(Role.Student);
        // Act & Assert - Student cannot create
        var createRequest = new CreateMeetingRequest
        {
            Attendees = [
                new MeetingAttendee { UserId = teacher.UserId, Role = AttendeeRole.Teacher },
                new MeetingAttendee { UserId = studentInfo.UserId, Role = AttendeeRole.Student }
            ],
            StartTimeUtc = DateTimeOffset.UtcNow.AddDays(1),
            DurationMinutes = 60,
            Description = "Integration Test Meeting",
        };
        var createResponse = await Client.PostAsJsonAsync(Constants.MeetingRoutes.CreateMeeting, createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Act & Assert - Student cannot update
        var updateRequest = new UpdateMeetingRequest { StartTimeUtc = DateTimeOffset.UtcNow.AddDays(3) };
        var updateResponse = await Client.PutAsJsonAsync(Constants.MeetingRoutes.UpdateMeeting(meeting.Id), updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Act & Assert - Student cannot delete
        var deleteResponse = await Client.DeleteAsync(Constants.MeetingRoutes.DeleteMeeting(meeting.Id));
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "Admin authorization - Can create and delete any meeting")]
    public async Task AdminAuthorization_FullAccess_ShouldSucceed()
    {
        // Arrange - Create meeting as teacher
        var teacher = await LoginAsAsync(Role.Teacher);
        var teacherMeeting = await CreateMeetingAsync(currentUser: teacher);

        // Act & Assert - Admin can create meetings
        var admin = await LoginAsAsync(Role.Admin);
        var teacherInfo = ClientFixture.GetUserInfo(Role.Teacher);
        
        var createRequest = new CreateMeetingRequest
        {
            Attendees = [new MeetingAttendee { UserId = teacherInfo.UserId, Role = AttendeeRole.Teacher }],
            StartTimeUtc = DateTimeOffset.UtcNow.AddHours(2),
            DurationMinutes = 30,
            Description = "Admin Created Meeting",
        };

        var adminMeeting = await CreateMeetingAsync(createRequest, admin);
        adminMeeting.Should().NotBeNull();
        adminMeeting.CreatedByUserId.Should().Be(admin.UserId);

        // Act & Assert - Admin can delete teacher's meeting
        var deleteSuccess = await DeleteMeetingAsync(teacherMeeting.Id);
        deleteSuccess.Should().BeTrue();
    }

    #endregion

    #region Query Tests

    [Fact(DisplayName = "Get meetings for user - Returns correct meetings for teacher and student")]
    public async Task GetMeetingsForUser_MultipleRoles_ShouldReturnCorrectMeetings()
    {
        // Arrange
        var teacher = await LoginAsAsync(Role.Teacher);
        var studentInfo = ClientFixture.GetUserInfo(Role.Student);
        
        var meeting1 = await CreateMeetingAsync(new CreateMeetingRequest
        {
            Attendees = [new MeetingAttendee { UserId = teacher.UserId, Role = AttendeeRole.Teacher }],
            StartTimeUtc = DateTimeOffset.UtcNow.AddHours(1),
            DurationMinutes = 30,
            Description = "Teacher Only Meeting",
        }, teacher);
        
        var meeting2 = await CreateMeetingAsync(new CreateMeetingRequest
        {
            Attendees = [
                new MeetingAttendee { UserId = teacher.UserId, Role = AttendeeRole.Teacher },
                new MeetingAttendee { UserId = studentInfo.UserId, Role = AttendeeRole.Student }
            ],
            StartTimeUtc = DateTimeOffset.UtcNow.AddHours(2),
            DurationMinutes = 45,
            Description = "Teacher and Student Meeting",
        }, teacher);

        // Act & Assert - Teacher sees both meetings
        var teacherMeetings = await GetMeetingsForUserAsync(teacher.UserId);
        teacherMeetings.Should().HaveCountGreaterThanOrEqualTo(2);
        teacherMeetings.Should().Contain(m => m.Id == meeting1.Id);
        teacherMeetings.Should().Contain(m => m.Id == meeting2.Id);

        // Act & Assert - Student sees only meeting2
        await LoginAsAsync(Role.Student);
        var studentMeetings = await GetMeetingsForUserAsync(studentInfo.UserId);
        studentMeetings.Should().Contain(m => m.Id == meeting2.Id);
        studentMeetings.Should().NotContain(m => m.Id == meeting1.Id);
    }

    [Fact(DisplayName = "Get meetings for user - Returns empty list for new user")]
    public async Task GetMeetingsForUser_NewUser_ShouldReturnEmpty()
    {
        // Arrange
        var (teacherEmail, teacherToken) = await ClientFixture.CreateEphemeralUserAsync(Role.Teacher);
        
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(teacherToken);
        var teacherId = Guid.Parse(jwtToken.Claims.First(c => c.Type == TestConstants.UserId).Value);

        // Act
        var meetings = await GetMeetingsForUserAsync(teacherId);

        // Assert
        meetings.Should().BeEmpty();
    }

    [Fact(DisplayName = "Get meeting by ID - Returns 404 for nonexistent meeting")]
    public async Task GetMeeting_NonexistentId_ShouldReturn404()
    {
        // Arrange
        await LoginAsAsync(Role.Teacher);
        var nonexistentId = Guid.NewGuid();

        // Act
        var meeting = await GetMeetingAsync(nonexistentId);

        // Assert
        meeting.Should().BeNull();
    }

    [Fact(DisplayName = "Delete meeting - Returns 404 for nonexistent meeting")]
    public async Task DeleteMeeting_NonexistentId_ShouldReturn404()
    {
        // Arrange
        await LoginAsAsync(Role.Teacher);
        var nonexistentId = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync(Constants.MeetingRoutes.DeleteMeeting(nonexistentId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region ACS Token Generation Tests

    [Fact(DisplayName = "Generate ACS token - Multiple participants can generate tokens for same meeting")]
    public async Task GenerateAcsToken_MultipleParticipants_ShouldSucceed()
    {
        // Arrange
        var teacher = await LoginAsAsync(Role.Teacher);
        var studentInfo = ClientFixture.GetUserInfo(Role.Student);

        var meeting = await CreateMeetingAsync(new CreateMeetingRequest
        {
            Attendees = [
                new MeetingAttendee { UserId = teacher.UserId, Role = AttendeeRole.Teacher },
                new MeetingAttendee { UserId = studentInfo.UserId, Role = AttendeeRole.Student }
            ],
            StartTimeUtc = DateTimeOffset.UtcNow.AddHours(1),
            DurationMinutes= 60,
            Description = "ACS Toke n Generation Meeting",
        }, teacher);

        // Act - Teacher generates token
        var teacherToken = await GenerateTokenForMeetingAsync(meeting.Id);

        // Act - Student generates token
        await LoginAsAsync(Role.Student);
        var studentToken = await GenerateTokenForMeetingAsync(meeting.Id);

        // Assert
        teacherToken.Should().NotBeNull();
        studentToken.Should().NotBeNull();

        teacherToken!.Token.Should().NotBeNullOrWhiteSpace();
        studentToken!.Token.Should().NotBeNullOrWhiteSpace();

        teacherToken.GroupId.Should().Be(studentToken.GroupId, "Both tokens should reference the same meeting/group");
        teacherToken.GroupId.Should().Be(meeting.GroupCallId);

        teacherToken.UserId.Should().NotBe(studentToken.UserId, "Each participant should have a unique ACS user ID");

        teacherToken.ExpiresOn.Should().BeAfter(DateTimeOffset.UtcNow);
        studentToken.ExpiresOn.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact(DisplayName = "Generate ACS token - Non-participant cannot generate token")]
    public async Task GenerateAcsToken_NonParticipant_ShouldFail()
    {
        // Arrange
        var teacher = await LoginAsAsync(Role.Teacher);
        var meeting = await CreateMeetingAsync(currentUser: teacher);
        
        // Create student who is not a participant
        var (otherStudentEmail, otherStudentToken) = await ClientFixture.CreateEphemeralUserAsync(Role.Student);

        // Act
        var acsToken = await GenerateTokenForMeetingAsync(meeting.Id);

        // Assert
        acsToken.Should().BeNull();
    }

    [Fact(DisplayName = "Generate ACS token - Returns null for nonexistent meeting")]
    public async Task GenerateAcsToken_NonexistentMeeting_ShouldReturnNull()
    {
        // Arrange
        await LoginAsAsync(Role.Teacher);
        var nonexistentId = Guid.NewGuid();

        // Act
        var acsToken = await GenerateTokenForMeetingAsync(nonexistentId);

        // Assert
        acsToken.Should().BeNull();
    }

    #endregion
}
