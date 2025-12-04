using System.Security.Claims;
using Manager.Constants;
using Manager.Models.Users;
using Manager.Services.PeriodSummary;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class SummaryEndpoints
{
    private sealed class SummaryEndpoint { }

    public static IEndpointRouteBuilder MapSummaryEndpoints(this IEndpointRouteBuilder app)
    {
        var summaryGroup = app.MapGroup("/summaries-manager").WithTags("Period Summary");

        summaryGroup.MapGet("summary/{userId:guid}/overview", GetPeriodOverviewAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        summaryGroup.MapGet("summary/{userId:guid}/game-practice", GetPeriodGamePracticeAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        summaryGroup.MapGet("summary/{userId:guid}/word-cards", GetPeriodWordCardsAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        summaryGroup.MapGet("summary/{userId:guid}/achievements", GetPeriodAchievementsAsync)
            .RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        return app;
    }

    private static async Task<IResult> GetPeriodOverviewAsync(
        [FromRoute] Guid userId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromServices] IPeriodSummarizerService periodSummaryService,
        HttpContext http,
        ILogger<SummaryEndpoint> logger,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            logger.LogWarning("Invalid userId provided");
            return Results.BadRequest("Invalid userId");
        }

        if (!IsAuthorized(http, userId, logger, out var authResult))
        {
            return authResult!;
        }

        try
        {
            var (start, end) = GetDateRange(startDate, endDate, logger);

            logger.LogInformation("Fetching period overview for UserId={UserId}, StartDate={StartDate}, EndDate={EndDate}",
                userId, start, end);

            var response = await periodSummaryService.GetPeriodOverviewAsync(userId, start, end, ct);

            logger.LogInformation("Period overview retrieved successfully for UserId={UserId}", userId);
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching period overview for UserId={UserId}", userId);
            return Results.Problem("Failed to fetch period overview. Please try again later.");
        }
    }

    private static async Task<IResult> GetPeriodGamePracticeAsync(
        [FromRoute] Guid userId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromServices] IPeriodSummarizerService periodSummaryService,
        HttpContext http,
        ILogger<SummaryEndpoint> logger,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            logger.LogWarning("Invalid userId provided");
            return Results.BadRequest("Invalid userId");
        }

        if (!IsAuthorized(http, userId, logger, out var authResult))
        {
            return authResult!;
        }

        try
        {
            var (start, end) = GetDateRange(startDate, endDate, logger);

            logger.LogInformation("Fetching period game practice for UserId={UserId}, StartDate={StartDate}, EndDate={EndDate}",
                userId, start, end);

            var response = await periodSummaryService.GetPeriodGamePracticeAsync(userId, start, end, ct);

            logger.LogInformation("Period game practice retrieved successfully for UserId={UserId}", userId);
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching period game practice for UserId={UserId}", userId);
            return Results.Problem("Failed to fetch period game practice. Please try again later.");
        }
    }

    private static async Task<IResult> GetPeriodWordCardsAsync(
        [FromRoute] Guid userId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromServices] IPeriodSummarizerService periodSummaryService,
        HttpContext http,
        ILogger<SummaryEndpoint> logger,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            logger.LogWarning("Invalid userId provided");
            return Results.BadRequest("Invalid userId");
        }

        if (!IsAuthorized(http, userId, logger, out var authResult))
        {
            return authResult!;
        }

        try
        {
            var (start, end) = GetDateRange(startDate, endDate, logger);

            logger.LogInformation("Fetching period word cards for UserId={UserId}, StartDate={StartDate}, EndDate={EndDate}",
                userId, start, end);

            var response = await periodSummaryService.GetPeriodWordCardsAsync(userId, start, end, ct);

            logger.LogInformation("Period word cards retrieved successfully for UserId={UserId}", userId);
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching period word cards for UserId={UserId}", userId);
            return Results.Problem("Failed to fetch period word cards. Please try again later.");
        }
    }

    private static async Task<IResult> GetPeriodAchievementsAsync(
        [FromRoute] Guid userId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromServices] IPeriodSummarizerService periodSummaryService,
        HttpContext http,
        ILogger<SummaryEndpoint> logger,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
        {
            logger.LogWarning("Invalid userId provided");
            return Results.BadRequest("Invalid userId");
        }

        if (!IsAuthorized(http, userId, logger, out var authResult))
        {
            return authResult!;
        }

        try
        {
            var (start, end) = GetDateRange(startDate, endDate, logger);

            logger.LogInformation("Fetching period achievements for UserId={UserId}, StartDate={StartDate}, EndDate={EndDate}",
                userId, start, end);

            var response = await periodSummaryService.GetPeriodAchievementsAsync(userId, start, end, ct);

            logger.LogInformation("Period achievements retrieved successfully for UserId={UserId}", userId);
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching period achievements for UserId={UserId}", userId);
            return Results.Problem("Failed to fetch period achievements. Please try again later.");
        }
    }

    private static bool IsAuthorized(HttpContext http, Guid targetUserId, ILogger logger, out IResult? result)
    {
        result = null;
        var callerRole = http.User.FindFirstValue(AuthSettings.RoleClaimType);
        var callerIdRaw = http.User.FindFirstValue(AuthSettings.UserIdClaimType);

        if (string.IsNullOrWhiteSpace(callerRole))
        {
            logger.LogWarning("Unauthorized: missing role");
            result = Results.Unauthorized();
            return false;
        }

        if (!Guid.TryParse(callerIdRaw, out var callerId))
        {
            logger.LogWarning("Unauthorized: missing or invalid caller ID");
            result = Results.Unauthorized();
            return false;
        }

        if (callerRole.Equals(Role.Student.ToString(), StringComparison.OrdinalIgnoreCase) && callerId != targetUserId)
        {
            logger.LogWarning("Forbidden: Student {CallerId} tried to view data for {TargetUserId}", callerId, targetUserId);
            result = Results.Forbid();
            return false;
        }

        return true;
    }

    private static (DateTime start, DateTime end) GetDateRange(DateTime? startDate, DateTime? endDate, ILogger? logger = null)
    {
        var now = DateTime.UtcNow;

        if (!startDate.HasValue && !endDate.HasValue)
        {
            logger?.LogDebug("No dates provided, defaulting to last 7 days");
            return (now.AddDays(-7), now);
        }

        if (!startDate.HasValue && endDate.HasValue)
        {
            var end = endDate.Value > now ? now : endDate.Value;
            if (endDate.Value > now)
            {
                logger?.LogWarning("EndDate {EndDate} is in the future, capping to current time {Now}", endDate.Value, now);
            }

            return (end.AddDays(-7), end);
        }

        if (startDate.HasValue && !endDate.HasValue)
        {
            if (startDate.Value > now)
            {
                logger?.LogWarning("StartDate {StartDate} is in the future, resetting to last 7 days from now", startDate.Value);
                return (now.AddDays(-7), now);
            }

            return (startDate.Value, now);
        }

        var finalEnd = endDate!.Value > now ? now : endDate.Value;
        var finalStart = startDate!.Value > finalEnd ? finalEnd.AddDays(-7) : startDate.Value;

        if (endDate.Value > now)
        {
            logger?.LogWarning("EndDate {EndDate} is in the future, capping to current time {Now}", endDate.Value, now);
        }

        if (startDate.Value > finalEnd)
        {
            logger?.LogWarning("StartDate {StartDate} is after EndDate {EndDate}, adjusting to 7 days before EndDate", startDate.Value, finalEnd);
        }

        return (finalStart, finalEnd);
    }
}
