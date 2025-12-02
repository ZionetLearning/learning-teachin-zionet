using Manager.Services.Clients.Accessor.Models.Meetings;

namespace Manager.Services.Clients.Accessor.Interfaces;

public interface IMeetingAccessorClient
{
    Task<GetMeetingAccessorResponse?> GetMeetingAsync(Guid meetingId, CancellationToken ct = default);
    Task<IReadOnlyList<GetMeetingAccessorResponse>> GetMeetingsForUserAsync(Guid userId, CancellationToken ct = default);
    Task<CreateMeetingAccessorResponse> CreateMeetingAsync(CreateMeetingAccessorRequest request, CancellationToken ct = default);
    Task<bool> UpdateMeetingAsync(Guid meetingId, UpdateMeetingAccessorRequest request, CancellationToken ct = default);
    Task<bool> DeleteMeetingAsync(Guid meetingId, CancellationToken ct = default);
    Task<GenerateMeetingTokenAccessorResponse> GenerateTokenForMeetingAsync(Guid meetingId, Guid userId, CancellationToken ct = default);
    Task<CreateOrGetIdentityAccessorResponse> CreateOrGetIdentityAsync(Guid userId, CancellationToken ct = default);
}
