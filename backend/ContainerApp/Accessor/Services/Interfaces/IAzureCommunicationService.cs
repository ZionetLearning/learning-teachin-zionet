using Accessor.Models.AzureCommunicationService;

namespace Accessor.Services.Interfaces;

public interface IAzureCommunicationService
{
    /// <summary>
    /// Creates or retrieves an ACS identity for a user
    /// </summary>
    Task<AcsIdentityResponse> CreateOrGetIdentityAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Generates an access token for a user to join a specific meeting
    /// </summary>
    Task<AcsTokenResponse> GenerateTokenForMeetingAsync(Guid userId, Guid meetingId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new ACS room/group call for a meeting with time boundaries
    /// </summary>
    Task<string> CreateRoomAsync(DateTimeOffset startTime, DateTimeOffset endTime, CancellationToken ct = default);

    /// <summary>
    /// Deletes an ACS room (e.g., when meeting is cancelled)
    /// </summary>
    Task DeleteRoomAsync(string roomId, CancellationToken ct = default);
}
