using System.Security.Claims;
using Manager.Constants;
using Manager.Helpers;
using Manager.Mapping;
using Manager.Models.Meetings;
using Manager.Models.Meetings.Requests;
using Manager.Models.ModelValidation;
using Manager.Models.Users;
using Manager.Services.Clients.Accessor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Manager.Endpoints;

public static class MeetingsEndpoints
{
    private sealed class MeetingsEndpoint { }

    public static IEndpointRouteBuilder MapMeetingsEndpoints(this IEndpointRouteBuilder app)
    {
        var meetingsGroup = app.MapGroup("/meetings-manager").WithTags("Meetings");

        meetingsGroup.MapGet("/{meetingId:guid}", GetMeetingAsync)
            .WithName("GetMeeting")
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        meetingsGroup.MapGet("/user/{userId:guid}", GetMeetingsForUserAsync)
            .WithName("GetMeetingsForUser")
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        meetingsGroup.MapPost("", CreateMeetingAsync)
            .WithName("CreateMeeting")
            .RequireAuthorization(PolicyNames.AdminOrTeacher);

        meetingsGroup.MapPost("/{meetingId:guid}/token", GenerateTokenForMeetingAsync)
            .WithName("GenerateTokenForMeeting")
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        meetingsGroup.MapPut("/{meetingId:guid}", UpdateMeetingAsync)
            .WithName("UpdateMeeting")
            .RequireAuthorization(PolicyNames.AdminOrTeacher);

        meetingsGroup.MapDelete("/{meetingId:guid}", DeleteMeetingAsync)
            .WithName("DeleteMeeting")
            .RequireAuthorization(PolicyNames.AdminOrTeacher);

        return app;
    }

    private static async Task<IResult> GetMeetingAsync(
        [FromRoute] Guid meetingId,
        [FromServices] IMeetingAccessorClient meetingAccessorClient,
        [FromServices] ILogger<MeetingsEndpoint> logger,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (meetingId == Guid.Empty)
        {
            logger.LogWarning("Invalid meeting ID provided: {MeetingId}", meetingId);
            return Results.BadRequest("Invalid meeting ID.");
        }

        var callerIdRaw = httpContext.User.FindFirstValue(AuthSettings.UserIdClaimType);
        var callerRole = httpContext.User.FindFirstValue(AuthSettings.RoleClaimType);

        if (!Guid.TryParse(callerIdRaw, out var callerId))
        {
            logger.LogWarning("Invalid or missing caller ID");
            return Results.Unauthorized();
        }

        try
        {
            var accessorMeeting = await meetingAccessorClient.GetMeetingAsync(meetingId, ct);
            if (accessorMeeting is null)
            {
                return Results.NotFound();
            }

            // Check authorization: user must be an attendee or admin
            if (callerRole != Role.Admin.ToString())
            {
                var isAttendee = accessorMeeting.Attendees.Any(a => a.UserId == callerId);
                if (!isAttendee)
                {
                    logger.LogWarning("User {UserId} is not authorized to view meeting {MeetingId}", callerId, meetingId);
                    return Results.Forbid();
                }
            }

            var response = accessorMeeting.ToFront();
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve meeting.");
            return Results.Problem("An error occurred while retrieving the meeting.");
        }
    }

    private static async Task<IResult> GetMeetingsForUserAsync(
        [FromRoute] Guid userId,
        [FromServices] IMeetingAccessorClient meetingAccessorClient,
        [FromServices] ILogger<MeetingsEndpoint> logger,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            logger.LogWarning("Invalid user ID provided: {UserId}", userId);
            return Results.BadRequest("Invalid user ID.");
        }

        var callerIdRaw = httpContext.User.FindFirstValue(AuthSettings.UserIdClaimType);
        var callerRole = httpContext.User.FindFirstValue(AuthSettings.RoleClaimType);

        if (!Guid.TryParse(callerIdRaw, out var callerId))
        {
            logger.LogWarning("Invalid or missing caller ID");
            return Results.Unauthorized();
        }

        if (callerId != userId && callerRole != Role.Admin.ToString())
        {
            logger.LogWarning("User {CallerId} is not authorized to view meetings for user {UserId}", callerId, userId);
            return Results.Forbid();
        }

        try
        {
            var accessorMeetings = await meetingAccessorClient.GetMeetingsForUserAsync(userId, ct);
            logger.LogInformation("Retrieved {Count} meetings for user {UserId}", accessorMeetings.Count, userId);
            var response = accessorMeetings.ToFront();
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve meetings for user.");
            return Results.Problem("An error occurred while retrieving meetings.");
        }
    }

    private static async Task<IResult> CreateMeetingAsync(
        [FromBody] CreateMeetingRequest request,
        [FromServices] IMeetingAccessorClient meetingAccessorClient,
        [FromServices] IOptions<MeetingOptions> meetingOptions,
        [FromServices] ILogger<MeetingsEndpoint> logger,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (request is null)
        {
            logger.LogWarning("Meeting request is null.");
            return Results.BadRequest("Meeting data is required.");
        }

        if (!ValidationExtensions.TryValidate(request, out var validationErrors))
        {
            logger.LogWarning("Validation failed for {Model}: {Errors}", nameof(CreateMeetingRequest), validationErrors);
            return Results.BadRequest(new { errors = validationErrors });
        }

        var configErrors = MeetingValidationHelper.ValidateCreateMeetingRequest(request, meetingOptions.Value);
        if (configErrors.Any())
        {
            logger.LogWarning("Meeting configuration validation failed: {Errors}", string.Join("; ", configErrors));
            return Results.BadRequest(new { errors = configErrors });
        }

        if (request.Attendees == null || !request.Attendees.Any())
        {
            logger.LogWarning("Meeting must have at least one attendee.");
            return Results.BadRequest("Meeting must have at least one attendee.");
        }

        foreach (var attendee in request.Attendees)
        {
            if (!ValidationExtensions.TryValidate(attendee, out var attendeeErrors))
            {
                logger.LogWarning("Validation failed for attendee: {Errors}", attendeeErrors);
                return Results.BadRequest(new { errors = attendeeErrors });
            }
        }

        var callerIdRaw = httpContext.User.FindFirstValue(AuthSettings.UserIdClaimType);
        var callerRole = httpContext.User.FindFirstValue(AuthSettings.RoleClaimType);

        if (!Guid.TryParse(callerIdRaw, out var callerId))
        {
            logger.LogWarning("Invalid or missing caller ID");
            return Results.Unauthorized();
        }

        var createdByUserId = callerId;

        if (callerRole == Role.Teacher.ToString())
        {
            var callerIsAttendee = request.Attendees.Any(a => a.UserId == callerId && a.Role == AttendeeRole.Teacher);
            if (!callerIsAttendee)
            {
                logger.LogWarning("Teacher {CallerId} must be an attendee of the meeting they create", callerId);
                return Results.BadRequest("Teacher must be an attendee of the meeting.");
            }
        }
        else if (callerRole != Role.Admin.ToString())
        {
            logger.LogWarning("User {CallerId} with role {Role} is not authorized to create meetings", callerId, callerRole);
            return Results.Forbid();
        }

        try
        {
            foreach (var attendee in request.Attendees)
            {
                await meetingAccessorClient.CreateOrGetIdentityAsync(attendee.UserId, ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create or get ACS identities for attendees");
            return Results.Problem("Failed to create ACS identities for meeting attendees.");
        }

        try
        {
            var accessorRequest = request.ToAccessor(createdByUserId);
            var accessorMeeting = await meetingAccessorClient.CreateMeetingAsync(accessorRequest, ct);
            var response = accessorMeeting.ToFront();

            logger.LogInformation("Meeting {MeetingId} created successfully by user {UserId}", response.Id, createdByUserId);

            return Results.Created($"/meetings-manager/{response.Id}", response);
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
        [FromServices] IMeetingAccessorClient meetingAccessorClient,
        [FromServices] IOptions<MeetingOptions> meetingOptions,
        [FromServices] ILogger<MeetingsEndpoint> logger,
        HttpContext httpContext,
        CancellationToken ct)
    {
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

        if (!ValidationExtensions.TryValidate(request, out var validationErrors))
        {
            logger.LogWarning("Validation failed for {Model}: {Errors}", nameof(UpdateMeetingRequest), validationErrors);
            return Results.BadRequest(new { errors = validationErrors });
        }

        var configErrors = MeetingValidationHelper.ValidateUpdateMeetingRequest(request, meetingOptions.Value);
        if (configErrors.Any())
        {
            logger.LogWarning("Meeting configuration validation failed: {Errors}", string.Join("; ", configErrors));
            return Results.BadRequest(new { errors = configErrors });
        }

        if (request.Attendees != null)
        {
            foreach (var attendee in request.Attendees)
            {
                if (!ValidationExtensions.TryValidate(attendee, out var attendeeErrors))
                {
                    logger.LogWarning("Validation failed for attendee: {Errors}", attendeeErrors);
                    return Results.BadRequest(new { errors = attendeeErrors });
                }
            }
        }

        var callerIdRaw = httpContext.User.FindFirstValue(AuthSettings.UserIdClaimType);
        var callerRole = httpContext.User.FindFirstValue(AuthSettings.RoleClaimType);

        if (!Guid.TryParse(callerIdRaw, out var callerId))
        {
            logger.LogWarning("Invalid or missing caller ID");
            return Results.Unauthorized();
        }

        try
        {
            var existingMeeting = await meetingAccessorClient.GetMeetingAsync(meetingId, ct);
            if (existingMeeting is null)
            {
                return Results.NotFound("Meeting not found");
            }

            if (existingMeeting.CreatedByUserId != callerId && callerRole != Role.Admin.ToString())
            {
                logger.LogWarning("User {CallerId} is not authorized to update meeting {MeetingId}", callerId, meetingId);
                return Results.Forbid();
            }

            var accessorRequest = request.ToAccessor();
            var updated = await meetingAccessorClient.UpdateMeetingAsync(meetingId, accessorRequest, ct);
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
        [FromServices] IMeetingAccessorClient meetingAccessorClient,
        [FromServices] ILogger<MeetingsEndpoint> logger,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (meetingId == Guid.Empty)
        {
            logger.LogWarning("Invalid meeting ID provided: {MeetingId}", meetingId);
            return Results.BadRequest("Invalid meeting ID.");
        }

        var callerIdRaw = httpContext.User.FindFirstValue(AuthSettings.UserIdClaimType);
        var callerRole = httpContext.User.FindFirstValue(AuthSettings.RoleClaimType);

        if (!Guid.TryParse(callerIdRaw, out var callerId))
        {
            logger.LogWarning("Invalid or missing caller ID");
            return Results.Unauthorized();
        }

        try
        {
            var existingMeeting = await meetingAccessorClient.GetMeetingAsync(meetingId, ct);
            if (existingMeeting is null)
            {
                return Results.NotFound("Meeting not found");
            }

            if (existingMeeting.CreatedByUserId != callerId && callerRole != Role.Admin.ToString())
            {
                logger.LogWarning("User {CallerId} is not authorized to delete meeting {MeetingId}", callerId, meetingId);
                return Results.Forbid();
            }

            var deleted = await meetingAccessorClient.DeleteMeetingAsync(meetingId, ct);
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

    private static async Task<IResult> GenerateTokenForMeetingAsync(
        [FromRoute] Guid meetingId,
        [FromServices] IMeetingAccessorClient meetingAccessorClient,
        [FromServices] ILogger<MeetingsEndpoint> logger,
        HttpContext httpContext,
        CancellationToken ct)
    {
        if (meetingId == Guid.Empty)
        {
            logger.LogWarning("Invalid meeting ID provided: {MeetingId}", meetingId);
            return Results.BadRequest("Invalid meeting ID.");
        }

        var callerIdRaw = httpContext.User.FindFirstValue(AuthSettings.UserIdClaimType);

        if (!Guid.TryParse(callerIdRaw, out var callerId))
        {
            logger.LogWarning("Invalid or missing caller ID");
            return Results.Unauthorized();
        }

        try
        {
            var meeting = await meetingAccessorClient.GetMeetingAsync(meetingId, ct);
            if (meeting is null)
            {
                return Results.NotFound("Meeting not found");
            }

            var isAttendee = meeting.Attendees.Any(a => a.UserId == callerId);
            if (!isAttendee)
            {
                logger.LogWarning("User {UserId} is not an attendee of meeting {MeetingId}", callerId, meetingId);
                return Results.Forbid();
            }

            var accessorTokenResponse = await meetingAccessorClient.GenerateTokenForMeetingAsync(meetingId, callerId, ct);
            var response = accessorTokenResponse.ToFront();

            logger.LogInformation("Generated token for user {UserId} to join meeting {MeetingId}", callerId, meetingId);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "User {UserId} is not authorized to join meeting {MeetingId}", callerId, meetingId);
            return Results.Forbid();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
            return Results.NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate token for meeting.");
            return Results.Problem("An error occurred while generating the access token.");
        }
    }
}
