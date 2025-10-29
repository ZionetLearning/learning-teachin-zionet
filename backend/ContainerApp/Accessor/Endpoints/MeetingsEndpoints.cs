using Accessor.Models.Meetings;
using Accessor.Services;
using Accessor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class MeetingsEndpoints
{
    private sealed class MeetingsEndpointsLoggerMarker { }

    public static IEndpointRouteBuilder MapMeetingsEndpoints(this IEndpointRouteBuilder app)
    {
        var meetingsGroup = app.MapGroup("/meetings-accessor").WithTags("Meetings");

        meetingsGroup.MapGet("/{meetingId:guid}", GetMeetingAsync)
            .WithName("GetMeeting");

        meetingsGroup.MapGet("/user/{userId:guid}", GetMeetingsForUserAsync)
            .WithName("GetMeetingsForUser");

        meetingsGroup.MapPost("", CreateMeetingAsync)
            .WithName("CreateMeeting");

        meetingsGroup.MapPut("/{meetingId:guid}", UpdateMeetingAsync)
            .WithName("UpdateMeeting");

        meetingsGroup.MapDelete("/{meetingId:guid}", DeleteMeetingAsync)
            .WithName("DeleteMeeting");

        return app;
    }

    private static async Task<IResult> GetMeetingAsync(
        [FromRoute] Guid meetingId,
        [FromServices] IMeetingService meetingService,
        [FromServices] ILogger<MeetingService> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("Method: {Method}, MeetingId: {MeetingId}",
            nameof(GetMeetingAsync), meetingId);

        if (meetingId == Guid.Empty)
        {
            logger.LogWarning("Invalid meeting ID provided: {MeetingId}", meetingId);
            return Results.BadRequest("Invalid meeting ID.");
        }

        try
        {
            var meeting = await meetingService.GetMeetingAsync(meetingId, ct);
            return meeting is not null ? Results.Ok(meeting) : Results.NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve meeting.");
            return Results.Problem("An error occurred while retrieving the meeting.");
        }
    }

    private static async Task<IResult> GetMeetingsForUserAsync(
        [FromRoute] Guid userId,
        [FromServices] IMeetingService meetingService,
        [FromServices] ILogger<MeetingsEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("Method: {Method}, UserId: {UserId}",
            nameof(GetMeetingsForUserAsync), userId);

        if (userId == Guid.Empty)
        {
            logger.LogWarning("Invalid user ID provided: {UserId}", userId);
            return Results.BadRequest("Invalid user ID.");
        }

        try
        {
            var meetings = await meetingService.GetMeetingsForUserAsync(userId, ct);
            logger.LogInformation("Retrieved {Count} meetings for user {UserId}", meetings.Count, userId);
            return Results.Ok(meetings);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve meetings for user.");
            return Results.Problem("An error occurred while retrieving meetings.");
        }
    }

    private static async Task<IResult> CreateMeetingAsync(
        [FromBody] CreateMeetingRequest request,
        [FromServices] IMeetingService meetingService,
        [FromServices] ILogger<MeetingsEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("Method: {Method}", nameof(CreateMeetingAsync));

        if (request is null)
        {
            logger.LogWarning("Meeting request is null.");
            return Results.BadRequest("Meeting data is required.");
        }

        if (request.Attendees == null || !request.Attendees.Any())
        {
            logger.LogWarning("Meeting must have at least one attendee.");
            return Results.BadRequest("Meeting must have at least one attendee.");
        }

        if (request.GroupCallId == Guid.Empty)
        {
            logger.LogWarning("Invalid GroupCallId.");
            return Results.BadRequest("Valid GroupCallId is required.");
        }

        if (request.CreatedByUserId == Guid.Empty)
        {
            logger.LogWarning("Invalid CreatedByUserId.");
            return Results.BadRequest("Valid CreatedByUserId is required.");
        }

        try
        {
            var meeting = await meetingService.CreateMeetingAsync(request, ct);
            logger.LogInformation("Meeting {MeetingId} created successfully", meeting.Id);

            return Results.Created($"/meetings-accessor/{meeting.Id}", meeting);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create meeting.");
            return Results.Problem("An error occurred while creating the meeting.");
        }
    }

    private static async Task<IResult> UpdateMeetingAsync(
        [FromRoute] Guid meetingId,
        [FromBody] UpdateMeetingRequest request,
        [FromServices] IMeetingService meetingService,
        [FromServices] ILogger<MeetingsEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("Method: {Method}, MeetingId: {MeetingId}",
            nameof(UpdateMeetingAsync), meetingId);

        if (meetingId == Guid.Empty)
        {
            logger.LogWarning("Invalid meeting ID provided: {MeetingId}", meetingId);
            return Results.BadRequest("Invalid meeting ID.");
        }

        if (request is null)
        {
            logger.LogWarning("Update meeting request is null.");
            return Results.BadRequest("Meeting data is required.");
        }

        try
        {
            var updated = await meetingService.UpdateMeetingAsync(meetingId, request, ct);
            return updated
                ? Results.Ok("Meeting updated successfully")
                : Results.NotFound("Meeting not found");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to update meeting.");
            return Results.Problem("An error occurred while updating the meeting.");
        }
    }

    private static async Task<IResult> DeleteMeetingAsync(
        [FromRoute] Guid meetingId,
        [FromServices] IMeetingService meetingService,
        [FromServices] ILogger<MeetingsEndpointsLoggerMarker> logger,
        CancellationToken ct)
    {
        using var scope = logger.BeginScope("Method: {Method}, MeetingId: {MeetingId}",
            nameof(DeleteMeetingAsync), meetingId);

        if (meetingId == Guid.Empty)
        {
            logger.LogWarning("Invalid meeting ID provided: {MeetingId}", meetingId);
            return Results.BadRequest("Invalid meeting ID.");
        }

        try
        {
            var deleted = await meetingService.DeleteMeetingAsync(meetingId, ct);
            return deleted
                ? Results.Ok("Meeting deleted successfully")
                : Results.NotFound("Meeting not found");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete meeting.");
            return Results.Problem("An error occurred while deleting the meeting.");
        }
    }
}
