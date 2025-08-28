using Accessor.Models;
using Accessor.Services;
using Microsoft.AspNetCore.Mvc;

namespace Accessor.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/auth-accessor").WithTags("Auth");

        authGroup.MapPost("/login", LoginUserAsync).WithName("Login");

        return app;
    }

    private static async Task<IResult> LoginUserAsync(
        [FromBody] LoginRequest request,
        [FromServices] IAccessorService accessorService,
        [FromServices] ILogger<AccessorService> logger)
    {
        using var scope = logger.BeginScope("Handler: {Handler}, Email: {Email}", nameof(LoginUserAsync), request.Email);
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                logger.LogWarning("Email or password was empty.");
                return Results.BadRequest("Email and password are required.");
            }

            var userId = await accessorService.ValidateCredentialsAsync(request.Email, request.Password);
            if (userId == null)
            {
                logger.LogWarning("Invalid credentials for email: {Email}", request.Email);
                return Results.Unauthorized();
            }

            logger.LogInformation("User logged in successfully.");
            return Results.Ok(userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Login failed.");
            return Results.Problem("Internal error during login.");
        }
    }
}
