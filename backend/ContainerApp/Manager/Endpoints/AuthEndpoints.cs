using System.Security.Claims;
using Manager.Constants;
using Manager.Helpers;
using Manager.Models.Auth;
using Manager.Models.Auth.Erros;
using Manager.Services;
using Manager.Services.Clients.Accessor;
using Microsoft.AspNetCore.Mvc;

namespace Manager.Endpoints;

public static class AuthEndpoints
{
    private sealed class AuthEndpoint { }

    public static void MapAuthEndpoints(this WebApplication app)
    {
        #region Authentication and Authorization Endpoints

        var authGroup = app.MapGroup("/auth").WithTags("Auth");

        authGroup.MapPost("/login", LoginAsync).WithName("Login");

        authGroup.MapPost("/refresh-tokens", RefreshTokensAsync)
            .RequireRateLimiting(AuthSettings.RefreshTokenPolicy)
            .WithName("RefreshTokens");

        authGroup.MapPost("/logout", LogoutAsync).WithName("Logout");

        authGroup.MapGet("/protected", TestAuthAsync)
            .RequireAuthorization()
            .WithName("Protected");

        var maintenanceGroup = authGroup.MapGroup("/maintenance").WithTags("Maintenance");
        maintenanceGroup.MapPost("/refresh-sessions/cleanup", RefreshSessionsCleanupAsync)
            .WithName("Auth_RefreshSessionsCleanup");

        #endregion
    }
    #region Handlers
    private static async Task<IResult> LoginAsync(
       [FromBody] LoginRequest loginRequest,
       [FromServices] IAuthService authService,
       [FromServices] ILogger<AuthEndpoint> logger,
       HttpRequest httpRequest,
       HttpResponse response)
    {
        using var scope = logger.BeginScope("Method: {Method}", nameof(LoginAsync));
        try
        {
            logger.LogInformation("Attempting login for {Email}", loginRequest.Email);

            var (accessToken, refreshToken) = await authService.LoginAsync(loginRequest, httpRequest);

            // Set the cookies in the response
            var csrfToken = CookieHelper.SetCookies(response, refreshToken);

            logger.LogInformation("Login successful for {Email}", loginRequest.Email);
            return Results.Ok(new { accessToken, csrfToken });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning("Unauthorized login attempt for {Email}: {Message}", loginRequest.Email, ex.Message);
            return Results.Json(new ErrorResponse
            {
                Status = StatusCodes.Status401Unauthorized,
                Code = ErrorCodes.Unauthorized,
                Message = ex.Message
            }, statusCode: StatusCodes.Status401Unauthorized);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login for {Email}", loginRequest.Email);
            return Results.Json(new ErrorResponse
            {
                Status = StatusCodes.Status500InternalServerError,
                Code = ErrorCodes.InternalServerError,
                Message = "An unexpected error occurred. Please try again later."
            }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> RefreshTokensAsync(
        [FromServices] IAuthService authService,
        [FromServices] ILogger<AuthEndpoint> logger,
        HttpRequest request,
        HttpResponse response)
    {
        using var scope = logger.BeginScope("Method: {Method}", nameof(RefreshTokensAsync));
        try
        {
            var (accessToken, newRefreshToken) = await authService.RefreshTokensAsync(request);

            // Set again the cookies in the response
            var csrfToken = CookieHelper.SetCookies(response, newRefreshToken);

            logger.LogInformation("Refresh token successful");
            return Results.Ok(new { accessToken, csrfToken });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Refresh token request unauthorized");
            return Results.Json(new ErrorResponse
            {
                Status = StatusCodes.Status401Unauthorized,
                Code = ErrorCodes.Unauthorized,
                Message = ex.Message
            }, statusCode: StatusCodes.Status401Unauthorized);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing token");
            return Results.Json(new ErrorResponse
            {
                Status = StatusCodes.Status500InternalServerError,
                Code = ErrorCodes.InternalServerError,
                Message = "An unexpected error occurred. Please try again later."
            }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> LogoutAsync(
        [FromServices] IAuthService authService,
        [FromServices] ILogger<AuthEndpoint> logger,
        HttpRequest request,
        HttpResponse response)
    {
        using var scope = logger.BeginScope("Method: {Method}", nameof(LogoutAsync));
        try
        {
            await authService.LogoutAsync(request);

            // Clear the cookies in the response
            CookieHelper.ClearCookies(response);

            logger.LogInformation("Logout successful");
            return Results.Ok(new { message = "Logged out successfully" });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Logout request unauthorized");
            return Results.Json(new ErrorResponse
            {
                Status = StatusCodes.Status401Unauthorized,
                Code = ErrorCodes.Unauthorized,
                Message = ex.Message
            }, statusCode: StatusCodes.Status401Unauthorized);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout");
            return Results.Json(new ErrorResponse
            {
                Status = StatusCodes.Status500InternalServerError,
                Code = ErrorCodes.InternalServerError,
                Message = ex.Message
            }, statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static Task<IResult> TestAuthAsync(
        [FromServices] ILogger<AuthEndpoint> logger,
        HttpContext context)
    {
        using var scope = logger.BeginScope("Method: {Method}", nameof(TestAuthAsync));
        try
        {
            logger.LogInformation("You are authenticated!");
            var user = context.User;

            var userId = user.Identity?.Name; // because NameClaimType = "userid"
            var role = user.FindFirst(ClaimTypes.Role)?.Value;

            logger.LogInformation("Authenticated request. UserId: {UserId}, Role: {Role}", userId, role);

            return Task.FromResult(Results.Ok(new
            {
                message = "You are authenticated!",
                userId,
                role
            }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout");
            return Task.FromResult(Results.Problem("Auth test failed!"));
        }
    }

    private static async Task<IResult> RefreshSessionsCleanupAsync(
    [FromServices] IAccessorClient accessorClient,
    [FromServices] ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("Maintenance.RefreshSessionsCleanup");

        try
        {
            var deleted = await accessorClient.CleanupRefreshSessionsAsync(ct);
            logger.LogInformation("Cleanup done; deleted={Deleted}", deleted);
            return Results.Ok(new { deleted });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cleanup failed");
            return Results.Problem("Cleanup failed");
        }
    }

    #endregion
}
