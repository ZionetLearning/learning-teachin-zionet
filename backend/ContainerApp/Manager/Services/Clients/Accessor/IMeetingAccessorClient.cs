using Manager.Models.Meetings;

namespace Manager.Services.Clients.Accessor;

public interface IMeetingAccessorClient
{
    Task<MeetingDto?> GetMeetingAsync(Guid meetingId, CancellationToken ct = default);
    Task<IReadOnlyList<MeetingDto>> GetMeetingsForUserAsync(Guid userId, CancellationToken ct = default);
    Task<MeetingDto> CreateMeetingAsync(CreateMeetingRequest request, Guid createdByUserId, CancellationToken ct = default);
    Task<bool> UpdateMeetingAsync(Guid meetingId, UpdateMeetingRequest request, CancellationToken ct = default);
    Task<bool> DeleteMeetingAsync(Guid meetingId, CancellationToken ct = default);
    Task<AcsTokenResponse> GenerateTokenForMeetingAsync(Guid meetingId, Guid userId, CancellationToken ct = default);
    Task<AcsIdentityResponse> CreateOrGetIdentityAsync(Guid userId, CancellationToken ct = default);
}
