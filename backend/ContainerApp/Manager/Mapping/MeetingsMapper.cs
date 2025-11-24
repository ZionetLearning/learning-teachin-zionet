using Manager.Models.Meetings;
using Manager.Models.Meetings.Requests;
using Manager.Models.Meetings.Responses;
using Manager.Services.Clients.Accessor.Models.Meetings;

namespace Manager.Mapping;

/// <summary>
/// Provides mapping methods between frontend models and accessor models for Meetings domain
/// </summary>
public static class MeetingsMapper
{
    #region CreateMeeting Mappings

    /// <summary>
    /// Maps frontend CreateMeetingRequest to Accessor request
    /// </summary>
    public static CreateMeetingAccessorRequest ToAccessor(this CreateMeetingRequest request, Guid createdByUserId)
    {
        return new CreateMeetingAccessorRequest
        {
            Attendees = request.Attendees.Select(a => new AttendeeAccessorDto
            {
                UserId = a.UserId,
                Role = a.Role
            }).ToList(),
            StartTimeUtc = request.StartTimeUtc,
            DurationMinutes = request.DurationMinutes,
            Description = request.Description,
            CreatedByUserId = createdByUserId
        };
    }

    /// <summary>
    /// Maps Accessor CreateMeetingAccessorResponse to frontend CreateMeetingResponse
    /// </summary>
    public static CreateMeetingResponse ToFront(this CreateMeetingAccessorResponse accessorResponse)
    {
        return new CreateMeetingResponse
        {
            Id = accessorResponse.Id,
            Attendees = accessorResponse.Attendees.Select(a => new MeetingAttendee
            {
                UserId = a.UserId,
                Role = a.Role
            }).ToList(),
            StartTimeUtc = accessorResponse.StartTimeUtc,
            DurationMinutes = accessorResponse.DurationMinutes,
            Description = accessorResponse.Description,
            Status = accessorResponse.Status,
            GroupCallId = accessorResponse.GroupCallId,
            CreatedOn = accessorResponse.CreatedOn,
            CreatedByUserId = accessorResponse.CreatedByUserId
        };
    }

    #endregion

    #region GetMeeting Mappings

    /// <summary>
    /// Maps Accessor GetMeetingAccessorResponse to frontend GetMeetingResponse
    /// </summary>
    public static GetMeetingResponse ToFront(this GetMeetingAccessorResponse accessorResponse)
    {
        return new GetMeetingResponse
        {
            Id = accessorResponse.Id,
            Attendees = accessorResponse.Attendees.Select(a => new MeetingAttendee
            {
                UserId = a.UserId,
                Role = a.Role
            }).ToList(),
            StartTimeUtc = accessorResponse.StartTimeUtc,
            DurationMinutes = accessorResponse.DurationMinutes,
            Description = accessorResponse.Description,
            Status = accessorResponse.Status,
            GroupCallId = accessorResponse.GroupCallId,
            CreatedOn = accessorResponse.CreatedOn,
            CreatedByUserId = accessorResponse.CreatedByUserId
        };
    }

    #endregion

    #region GetUserMeetings Mappings

    /// <summary>
    /// Maps list of GetMeetingAccessorResponse to enumerable of GetMeetingResponse
    /// </summary>
    public static IEnumerable<GetMeetingResponse> ToFront(this IReadOnlyList<GetMeetingAccessorResponse> accessorMeetings)
    {
        return accessorMeetings.Select(m => m.ToFront());
    }

    #endregion

    #region UpdateMeeting Mappings

    /// <summary>
    /// Maps frontend UpdateMeetingRequest to Accessor request
    /// </summary>
    public static UpdateMeetingAccessorRequest ToAccessor(this UpdateMeetingRequest request)
    {
        return new UpdateMeetingAccessorRequest
        {
            Attendees = request.Attendees?.Select(a => new AttendeeAccessorDto
            {
                UserId = a.UserId,
                Role = a.Role
            }).ToList(),
            StartTimeUtc = request.StartTimeUtc,
            DurationMinutes = request.DurationMinutes,
            Description = request.Description,
            Status = request.Status
        };
    }

    #endregion

    #region GenerateMeetingToken Mappings

    /// <summary>
    /// Maps Accessor GenerateMeetingTokenAccessorResponse to frontend GenerateMeetingTokenResponse
    /// </summary>
    public static GenerateMeetingTokenResponse ToFront(this GenerateMeetingTokenAccessorResponse accessorResponse)
    {
        return new GenerateMeetingTokenResponse
        {
            UserId = accessorResponse.UserId,
            Token = accessorResponse.Token,
            ExpiresOn = accessorResponse.ExpiresOn,
            GroupId = accessorResponse.GroupId
        };
    }

    #endregion
}
