using Accessor.DB;
using Accessor.Models.Meetings;
using Accessor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;

public class MeetingService : IMeetingService
{
    private readonly AccessorDbContext _db;
    private readonly ILogger<MeetingService> _logger;
    private readonly IAzureCommunicationService _acsService;

    public MeetingService(
        AccessorDbContext db,
        ILogger<MeetingService> logger,
        IAzureCommunicationService acsService)
    {
        _db = db;
        _logger = logger;
        _acsService = acsService;
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
            var userMeetings = await _db.Meetings
                .AsNoTracking()
                .Where(m => m.Attendees.Any(a => a.UserId == userId))
                .OrderByDescending(m => m.StartTimeUtc)
                .Select(m => new MeetingDto
                {
                    Id = m.Id,
                    Attendees = m.Attendees,
                    StartTimeUtc = m.StartTimeUtc,
                    DurationMinutes = m.DurationMinutes,
                    Description = m.Description,
                    Status = m.Status,
                    GroupCallId = m.GroupCallId,
                    CreatedOn = m.CreatedOn,
                    CreatedByUserId = m.CreatedByUserId
                })
                .ToListAsync(ct);

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
            _logger.LogInformation("Creating new ACS room for meeting");

            var validFrom = request.StartTimeUtc;
            var validUntil = request.StartTimeUtc.AddMinutes(request.DurationMinutes);

            var groupCallId = await _acsService.CreateRoomAsync(validFrom, validUntil, ct);

            var meeting = new MeetingModel
            {
                Id = Guid.NewGuid(),
                Attendees = request.Attendees,
                StartTimeUtc = request.StartTimeUtc,
                DurationMinutes = request.DurationMinutes,
                Description = request.Description,
                GroupCallId = groupCallId,
                Status = MeetingStatus.Scheduled,
                CreatedOn = DateTimeOffset.UtcNow,
                CreatedByUserId = request.CreatedByUserId
            };

            _db.Meetings.Add(meeting);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("CreateMeeting END: created meeting {MeetingId} with ACS Room {GroupCallId}, Duration={Duration}min",
                meeting.Id, groupCallId, request.DurationMinutes);
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

            if (request.Attendees != null)
            {
                meeting.Attendees = request.Attendees;
            }

            if (request.StartTimeUtc.HasValue)
            {
                meeting.StartTimeUtc = request.StartTimeUtc.Value;
            }

            if (request.DurationMinutes.HasValue)
            {
                meeting.DurationMinutes = request.DurationMinutes.Value;
            }

            if (request.Description != null)
            {
                meeting.Description = request.Description;
            }

            if (request.Status.HasValue)
            {
                meeting.Status = request.Status.Value;
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

            try
            {
                await _acsService.DeleteRoomAsync(meeting.GroupCallId, ct);
                _logger.LogInformation("Deleted ACS room {GroupCallId} for meeting {MeetingId}", meeting.GroupCallId, meetingId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete ACS room {GroupCallId}, continuing with meeting deletion", meeting.GroupCallId);
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
            DurationMinutes = meeting.DurationMinutes,
            Description = meeting.Description,
            Status = meeting.Status,
            GroupCallId = meeting.GroupCallId,
            CreatedOn = meeting.CreatedOn,
            CreatedByUserId = meeting.CreatedByUserId
        };
    }
}
