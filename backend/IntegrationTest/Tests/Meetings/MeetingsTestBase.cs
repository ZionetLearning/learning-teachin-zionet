using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using Manager.Models.Users;
using Manager.Models.Meetings;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Meetings;

[Collection("IntegrationTests")]
public abstract class MeetingsTestBase(
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
    /// Logs in as a predefined test user of the specified role.
    /// Returns UserInfo for the logged-in user.
    /// </summary>
    protected async Task<UserInfo> LoginAsAsync(Role role)
    {
        await ClientFixture.LoginAsync(role);
        await EnsureSignalRStartedAsync();
        return ClientFixture.GetUserInfo(role);
    }

    /// <summary>
    /// Creates a meeting with default or specified parameters.
    /// Automatically includes the currently logged-in user as a teacher attendee if not specified.
    /// </summary>
    protected async Task<MeetingDto> CreateMeetingAsync(
        CreateMeetingRequest? request = null,
        UserInfo? currentUser = null)
    {
        // Extract current user ID from JWT token if not provided
        if (currentUser == null && Client.DefaultRequestHeaders.Authorization != null)
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var token = Client.DefaultRequestHeaders.Authorization.Parameter;
            if (!string.IsNullOrEmpty(token))
            {
                var jwtToken = handler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == TestConstants.UserId)?.Value;
                if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
                {
                    var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
                    var role = Enum.TryParse<Role>(roleClaim, true, out var r) ? r : Role.Teacher;
                    currentUser = new UserInfo(userId, "", role);
                }
            }
        }

        request ??= new CreateMeetingRequest
        {
            Attendees = currentUser != null
                ? [new MeetingAttendee { UserId = currentUser.UserId, Role = currentUser.Role == Role.Teacher ? AttendeeRole.Teacher : AttendeeRole.Student }]
                : [],
            StartTimeUtc = DateTimeOffset.UtcNow.AddHours(1),
            DurationMinutes = 60,
            Description = "Test Meeting",
        };

        // Ensure the current user is included as an attendee if they're a teacher
        if (currentUser?.Role == Role.Teacher && !request.Attendees.Any(a => a.UserId == currentUser.UserId))
        {
            var updatedAttendees = request.Attendees.ToList();
            updatedAttendees.Insert(0, new MeetingAttendee { UserId = currentUser.UserId, Role = AttendeeRole.Teacher });
            request = request with { Attendees = updatedAttendees };
        }

        var response = await Client.PostAsJsonAsync(MeetingRoutes.CreateMeeting, request);
        response.EnsureSuccessStatusCode();

        var meeting = await ReadAsJsonAsync<MeetingDto>(response);
        return meeting ?? throw new InvalidOperationException("Failed to create meeting");
    }

    /// <summary>
    /// Gets a meeting by ID.
    /// </summary>
    protected async Task<MeetingDto?> GetMeetingAsync(Guid meetingId)
    {
        var response = await Client.GetAsync(MeetingRoutes.GetMeeting(meetingId));
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
            
        response.EnsureSuccessStatusCode();
        return await ReadAsJsonAsync<MeetingDto>(response);
    }

    /// <summary>
    /// Gets all meetings for a user.
    /// </summary>
    protected async Task<List<MeetingDto>> GetMeetingsForUserAsync(Guid userId)
    {
        var response = await Client.GetAsync(MeetingRoutes.GetMeetingsForUser(userId));
        response.EnsureSuccessStatusCode();
        
        var meetings = await ReadAsJsonAsync<List<MeetingDto>>(response);
        return meetings ?? [];
    }

    /// <summary>
    /// Updates a meeting.
    /// </summary>
    protected async Task<bool> UpdateMeetingAsync(Guid meetingId, UpdateMeetingRequest request)
    {
        var response = await Client.PutAsJsonAsync(MeetingRoutes.UpdateMeeting(meetingId), request);
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Deletes a meeting.
    /// </summary>
    protected async Task<bool> DeleteMeetingAsync(Guid meetingId)
    {
        var response = await Client.DeleteAsync(MeetingRoutes.DeleteMeeting(meetingId));
        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Generates an ACS token for a meeting participant.
    /// </summary>
    protected async Task<AcsTokenResponse?> GenerateTokenForMeetingAsync(Guid meetingId)
    {
        var response = await Client.PostAsync(MeetingRoutes.GenerateToken(meetingId), null);
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound || 
            response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            return null;
            
        response.EnsureSuccessStatusCode();
        return await ReadAsJsonAsync<AcsTokenResponse>(response);
    }

    /// <summary>
    /// Sets up a teacher-student relationship.
    /// </summary>
    protected async Task AssignStudentToTeacherAsync(Guid teacherId, Guid studentId)
    {
        var response = await Client.PostAsync(MappingRoutes.Assign(teacherId, studentId), null);
        response.EnsureSuccessStatusCode();
    }
}
