using Accessor.DB;
using Accessor.Models.AzureCommunicationService;
using Accessor.Services.Interfaces;
using Azure;
using Azure.Communication;
using Azure.Communication.Identity;
using Azure.Communication.Rooms;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;

public class AzureCommunicationService : IAzureCommunicationService
{
    private readonly CommunicationIdentityClient _identityClient;
    private readonly RoomsClient _roomsClient;
    private readonly ILogger<AzureCommunicationService> _logger;
    private readonly AccessorDbContext _db;

    public AzureCommunicationService(
        IConfiguration configuration,
        ILogger<AzureCommunicationService> logger,
        AccessorDbContext db)
    {
        var connectionString = configuration["CommunicationService:ConnectionString"]
            ?? throw new InvalidOperationException("CommunicationService:ConnectionString is not configured");

        _identityClient = new CommunicationIdentityClient(connectionString);
        _roomsClient = new RoomsClient(connectionString);
        _logger = logger;
        _db = db;
    }

    public async Task<AcsIdentityResponse> CreateOrGetIdentityAsync(Guid userId, CancellationToken ct = default)
    {
        _logger.LogInformation("CreateOrGetIdentity START (userId={UserId})", userId);

        try
        {
            // Check if user already has an ACS identity
            var user = await _db.Users.FindAsync([userId], cancellationToken: ct);

            if (user == null)
            {
                _logger.LogError("User not found (userId={UserId})", userId);
                throw new InvalidOperationException($"User {userId} not found");
            }

            // If user already has an ACS identity, return it
            if (!string.IsNullOrWhiteSpace(user.AcsUserId))
            {
                _logger.LogInformation("User {UserId} already has ACS identity: {AcsUserId}", userId, user.AcsUserId);
                return new AcsIdentityResponse { AcsUserId = user.AcsUserId };
            }

            // Create new ACS identity
            _logger.LogInformation("Creating new ACS identity for user {UserId}", userId);
            var identityResponse = await _identityClient.CreateUserAsync(ct);
            var acsUserId = identityResponse.Value.Id;

            _logger.LogInformation("Created ACS identity {AcsUserId} for user {UserId}", acsUserId, userId);

            // Store ACS identity in user record
            user.AcsUserId = acsUserId;
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("CreateOrGetIdentity END (userId={UserId}, acsUserId={AcsUserId})", userId, acsUserId);

            return new AcsIdentityResponse { AcsUserId = acsUserId };
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "ACS API error while creating identity for user {UserId}. Status: {Status}, ErrorCode: {ErrorCode}",
                userId, ex.Status, ex.ErrorCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateOrGetIdentity FAILED (userId={UserId})", userId);
            throw;
        }
    }

    public async Task<AcsTokenResponse> GenerateTokenForMeetingAsync(Guid userId, Guid meetingId, CancellationToken ct = default)
    {
        _logger.LogInformation("GenerateTokenForMeeting START (userId={UserId}, meetingId={MeetingId})", userId, meetingId);

        try
        {
            // Verify user exists and has access to the meeting
            var meeting = await _db.Meetings
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == meetingId, ct);

            if (meeting == null)
            {
                _logger.LogError("Meeting not found (meetingId={MeetingId})", meetingId);
                throw new InvalidOperationException($"Meeting {meetingId} not found");
            }

            // Check if user is an attendee
            var isAttendee = meeting.Attendees.Any(a => a.UserId == userId);
            if (!isAttendee)
            {
                _logger.LogError("User {UserId} is not an attendee of meeting {MeetingId}", userId, meetingId);
                throw new UnauthorizedAccessException($"User {userId} is not authorized to join meeting {meetingId}");
            }

            // Get or create ACS identity for user
            var identityResponse = await CreateOrGetIdentityAsync(userId, ct);
            var communicationUser = new CommunicationUserIdentifier(identityResponse.AcsUserId);

            // Generate access token with VoIP scope (valid for 24 hours by default)
            _logger.LogInformation("Generating access token for user {UserId} (AcsUserId={AcsUserId})", userId, identityResponse.AcsUserId);

            var tokenScopes = new[] { CommunicationTokenScope.VoIP };
            var tokenResponse = await _identityClient.GetTokenAsync(communicationUser, tokenScopes, ct);

            _logger.LogInformation("Generated access token for user {UserId}, expires at {ExpiresOn}", userId, tokenResponse.Value.ExpiresOn);

            return new AcsTokenResponse
            {
                UserId = identityResponse.AcsUserId,
                Token = tokenResponse.Value.Token,
                ExpiresOn = tokenResponse.Value.ExpiresOn,
                GroupId = meeting.GroupCallId
            };
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "ACS API error while generating token for user {UserId}, meeting {MeetingId}. Status: {Status}, ErrorCode: {ErrorCode}",
                userId, meetingId, ex.Status, ex.ErrorCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GenerateTokenForMeeting FAILED (userId={UserId}, meetingId={MeetingId})", userId, meetingId);
            throw;
        }
    }

    public async Task<string> CreateRoomAsync(DateTimeOffset startTime, DateTimeOffset endTime, CancellationToken ct = default)
    {
        _logger.LogInformation("CreateRoom START (startTime={StartTime}, endTime={EndTime})", startTime, endTime);

        try
        {
            var roomOptions = new CreateRoomOptions
            {
                ValidFrom = startTime,
                ValidUntil = endTime,
                PstnDialOutEnabled = false
            };

            var roomResponse = await _roomsClient.CreateRoomAsync(roomOptions, ct);
            var roomId = roomResponse.Value.Id;

            _logger.LogInformation("Created ACS room with ID {RoomId}", roomId);

            return roomId;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "ACS API error while creating room. Status: {Status}, ErrorCode: {ErrorCode}",
                ex.Status, ex.ErrorCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateRoom FAILED");
            throw;
        }
    }

    public async Task DeleteRoomAsync(string roomId, CancellationToken ct = default)
    {
        _logger.LogInformation("DeleteRoom START (roomId={RoomId})", roomId);

        try
        {
            await _roomsClient.DeleteRoomAsync(roomId, ct);

            _logger.LogInformation("Deleted ACS room {RoomId}", roomId);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning("Room {RoomId} not found (already deleted or never existed)", roomId);
            // Don't throw on 404 - room is already gone
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "ACS API error while deleting room {RoomId}. Status: {Status}, ErrorCode: {ErrorCode}",
                roomId, ex.Status, ex.ErrorCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteRoom FAILED (roomId={RoomId})", roomId);
            throw;
        }
    }
}
