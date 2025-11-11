using Accessor.Models.Meetings;

namespace Accessor.Services.Interfaces;

public interface IMeetingService
{
    Task<MeetingDto?> GetMeetingAsync(Guid meetingId, CancellationToken ct = default);
    Task<IReadOnlyList<MeetingDto>> GetMeetingsForUserAsync(Guid userId, CancellationToken ct = default);
    Task<MeetingDto> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken ct = default);
    Task<bool> UpdateMeetingAsync(Guid meetingId, UpdateMeetingRequest request, CancellationToken ct = default);
    Task<bool> DeleteMeetingAsync(Guid meetingId, CancellationToken ct = default);
}
