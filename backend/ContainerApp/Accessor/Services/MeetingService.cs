using Accessor.DB;
using Accessor.Models.Meetings;
using Accessor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;

public class MeetingService : IMeetingService
{
    private readonly AccessorDbContext _db;
    private readonly ILogger<MeetingService> _logger;

    public MeetingService(AccessorDbContext db, ILogger<MeetingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<MeetingDto?> GetMeetingAsync(Guid meetingId, CancellationToken ct = default)
    {
        _logger.LogInformation("GetMeeting START (meetingId={MeetingId})", meetingId);

        try
        {
            var meeting = await _db.Meetings
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == meetingId, ct);

            if (meeting == null)
            {
                _logger.LogWarning("Meeting not found (meetingId={MeetingId})", meetingId);
                return null;
            }

            var dto = MapToDto(meeting);
            _logger.LogInformation("GetMeeting END: returned meeting {MeetingId}", meetingId);
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMeeting FAILED (meetingId={MeetingId})", meetingId);
            throw;
        }
    }

    public async Task<IReadOnlyList<MeetingDto>> GetMeetingsForUserAsync(Guid userId, CancellationToken ct = default)
    {
        _logger.LogInformation("GetMeetingsForUser START (userId={UserId})", userId);

        try
        {
            // Get all meetings where the user is an attendee
            var meetings = await _db.Meetings
                .AsNoTracking()
                .ToListAsync(ct);

            // Filter meetings where the user is in the Attendees list
            var userMeetings = meetings
                .Where(m => m.Attendees.Any(a => a.UserId == userId))
                .Select(MapToDto)
                .OrderByDescending(m => m.StartTimeUtc)
                .ToList();

            _logger.LogInformation("GetMeetingsForUser END: returned {Count} meetings for user {UserId}",
                userMeetings.Count, userId);
            return userMeetings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMeetingsForUser FAILED (userId={UserId})", userId);
            throw;
        }
    }

    public async Task<MeetingDto> CreateMeetingAsync(CreateMeetingRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("CreateMeeting START (createdByUserId={CreatedByUserId})", request.CreatedByUserId);

        try
        {
            var meeting = new MeetingModel
            {
                Id = Guid.NewGuid(),
                Attendees = request.Attendees,
                StartTimeUtc = request.StartTimeUtc,
                GroupCallId = request.GroupCallId,
                Status = MeetingStatus.Scheduled,
                CreatedOn = DateTimeOffset.UtcNow,
                CreatedByUserId = request.CreatedByUserId
            };

            _db.Meetings.Add(meeting);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("CreateMeeting END: created meeting {MeetingId}", meeting.Id);
            return MapToDto(meeting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateMeeting FAILED (createdByUserId={CreatedByUserId})", request.CreatedByUserId);
            throw;
        }
    }

    public async Task<bool> UpdateMeetingAsync(Guid meetingId, UpdateMeetingRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("UpdateMeeting START (meetingId={MeetingId})", meetingId);

        try
        {
            var meeting = await _db.Meetings.FirstOrDefaultAsync(m => m.Id == meetingId, ct);

            if (meeting == null)
            {
                _logger.LogWarning("Meeting not found for update (meetingId={MeetingId})", meetingId);
                return false;
            }

            // Update only provided fields
            if (request.Attendees != null)
            {
                meeting.Attendees = request.Attendees;
            }

            if (request.StartTimeUtc.HasValue)
            {
                meeting.StartTimeUtc = request.StartTimeUtc.Value;
            }

            if (request.Status.HasValue)
            {
                meeting.Status = request.Status.Value;
            }

            if (request.GroupCallId.HasValue)
            {
                meeting.GroupCallId = request.GroupCallId.Value;
            }

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("UpdateMeeting END: updated meeting {MeetingId}", meetingId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateMeeting FAILED (meetingId={MeetingId})", meetingId);
            throw;
        }
    }

    public async Task<bool> DeleteMeetingAsync(Guid meetingId, CancellationToken ct = default)
    {
        _logger.LogInformation("DeleteMeeting START (meetingId={MeetingId})", meetingId);

        try
        {
            var meeting = await _db.Meetings.FirstOrDefaultAsync(m => m.Id == meetingId, ct);

            if (meeting == null)
            {
                _logger.LogWarning("Meeting not found for deletion (meetingId={MeetingId})", meetingId);
                return false;
            }

            _db.Meetings.Remove(meeting);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("DeleteMeeting END: deleted meeting {MeetingId}", meetingId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteMeeting FAILED (meetingId={MeetingId})", meetingId);
            throw;
        }
    }

    private static MeetingDto MapToDto(MeetingModel meeting)
    {
        return new MeetingDto
        {
            Id = meeting.Id,
            Attendees = meeting.Attendees,
            StartTimeUtc = meeting.StartTimeUtc,
            Status = meeting.Status,
            GroupCallId = meeting.GroupCallId,
            CreatedOn = meeting.CreatedOn,
            CreatedByUserId = meeting.CreatedByUserId
        };
    }
}
