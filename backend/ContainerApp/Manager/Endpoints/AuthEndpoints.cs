using Manager.Helpers;
using Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Manager.Models.Auth;

namespace Manager.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        #region Authentication and Authorization Endpoints

        app.MapPost("/api/auth/login", LoginAsync).WithName("Login");

        app.MapPost("/api/auth/refresh-tokens", RefreshTokensAsync).WithName("RefreshTokens");

        app.MapPost("/api/auth/logout", LogoutAsync).WithName("Logout");

        app.MapGet("/api/protected", TestAuthAsync)
        .RequireAuthorization();

        #endregion
    }
    #region Handlers
    private static async Task<IResult> LoginAsync(
       [FromBody] LoginRequest loginRequest,
       [FromServices] IAuthService authService,
       [FromServices] ILogger<ManagerService> logger,
       HttpRequest httpRequest,
       HttpResponse response,
       CancellationToken cancellationToken)
    {
        using (logger.BeginScope("Method: {Method}", nameof(LoginAsync)))
        {
            try
            {
                logger.LogInformation("Attempting login for {Email}", loginRequest.Email);

                var (accessToken, refreshToken) = await authService.LoginAsync(loginRequest, httpRequest, cancellationToken);

                // Set the cookies in the response
                CookieHelper.SetCookies(response, refreshToken);

                logger.LogInformation("Login successful for {Email}", loginRequest.Email);
                return Results.Ok(new { accessToken });
            }
            catch (UnauthorizedAccessException)
            {
                logger.LogWarning("Unauthorized login attempt for {Email}", loginRequest.Email);
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during login for {Email}", loginRequest.Email);
                return Results.Problem("An unexpected error occurred during login.");
            }
        }
    }

    private static async Task<IResult> RefreshTokensAsync(
        [FromServices] IAuthService authService,
        [FromServices] ILogger<ManagerService> logger,
        HttpRequest request,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        using (logger.BeginScope("Method: {Method}", nameof(RefreshTokensAsync)))
        {
            try
            {
                var (accessToken, newRefreshToken) = await authService.RefreshTokensAsync(request, cancellationToken);

                // Set again the cookies in the response
                CookieHelper.SetCookies(response, newRefreshToken);

                logger.LogInformation("Refresh token successful");
                return Results.Ok(new { accessToken });
            }
            catch (UnauthorizedAccessException)
            {
                logger.LogWarning("Refresh token request unauthorized");
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error refreshing token");
                return Results.Problem("Failed to refresh token");
            }
        }
    }

    private static async Task<IResult> LogoutAsync(
        [FromServices] IAuthService authService,
        [FromServices] ILogger<ManagerService> logger,
        HttpRequest request,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        using (logger.BeginScope("Method: {Method}", nameof(LogoutAsync)))
        {
            try
            {
                await authService.LogoutAsync(request, cancellationToken);

                // Clear the cookies in the response
                CookieHelper.ClearCookies(response);

                logger.LogInformation("Logout successful");
                return Results.Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during logout");
                return Results.Problem("An error occurred during logout.");
            }
        }
    }

    private static Task<IResult> TestAuthAsync(
        [FromServices] IAuthService authService,
        [FromServices] ILogger<ManagerService> logger,
        HttpRequest request,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        using (logger.BeginScope("Method: {Method}", nameof(TestAuthAsync)))
        {
            try
            {
                logger.LogInformation("You are authenticated!");
                return Task.FromResult(Results.Ok(new { message = "You are authenticated!" }));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during logout");
                return Task.FromResult(Results.Problem("Auth test failed!"));
            }
        }
    }
    #endregion
}
