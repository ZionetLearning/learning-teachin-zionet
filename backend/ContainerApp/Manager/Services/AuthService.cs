//using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapr.Client;
using Manager.Models;
using Manager.Models.Auth;
//using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Manager.Services;

public class AuthService : IAuthService
{
    private readonly DaprClient _dapr;
    private readonly ILogger<AuthService> _log;
    private readonly JwtSettings _jwt;
    private static readonly Dictionary<string, string> _fakeUsers = new()
    {
        { "1", "1" }
    };

    public AuthService(DaprClient dapr, ILogger<AuthService> log, IOptions<JwtSettings> jwtOptions)
    {
        _dapr = dapr ?? throw new ArgumentNullException(nameof(dapr));
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _jwt = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
    }

    public async Task<(string, string)> LoginAsync(LoginRequest loginRequest, HttpRequest httpRequest, CancellationToken cancellationToken)
    {
        try
        {
            if (!_fakeUsers.TryGetValue(loginRequest.Email, out var storedPassword) || storedPassword != loginRequest.Password)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var accessToken = GenerateJwtToken(loginRequest.Email);
            var refreshToken = Guid.NewGuid().ToString("N");

            // Send the refresh token to the save in the accessor

            // Collect session fingerprint data
            var fingerprint = httpRequest.Headers["x-fingerprint"].ToString();
            var userAgent = httpRequest.Headers.UserAgent.ToString();
            var ip = httpRequest.HttpContext.Connection.RemoteIpAddress?.ToString();

            // Save session in "auth-store" -> next task

            //await _dapr.InvokeMethodAsync("auth-store", "store-refresh-token", new
            //{
            //    Email = loginRequest.Email,
            //    RefreshToken = refreshToken,
            //    Fingerprint = fingerprint,
            //    UserAgent = userAgent,
            //    IP = ip
            //});

            // For now just call unrelated method in the accessor
            try
            {
                var task = await _dapr.InvokeMethodAsync<TaskModel?>(HttpMethod.Get, "accessor", $"task/2"
, cancellationToken);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error invoking method on accessor service");
            }

            return (accessToken, refreshToken);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Login failed for user {Email}", loginRequest.Email);
            throw new UnauthorizedAccessException("Login failed. Please check your credentials.", ex);
        }
    }

    public async Task<(string accessToken, string refreshToken)> RefreshTokensAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the old refresh token from the request cookies
            var oldRefreshToken = request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(oldRefreshToken))
            {
                throw new UnauthorizedAccessException("Missing refresh token.");
            }

            // Collect session fingerprint data
            var fingerprint = request.Headers["x-fingerprint"].ToString();
            var userAgent = request.Headers.UserAgent.ToString();
            var ip = request.HttpContext.Connection.RemoteIpAddress?.ToString();

            // Get session from Dapr
            // For now just call unrelated method in the accessor
            var session = await _dapr.InvokeMethodAsync<TaskModel?>(
                                HttpMethod.Get,
                                "accessor",
                                $"task/2",
                                cancellationToken
                            );

            //if (session == null || session.Fingerprint != fingerprint || session.IP != ip || session.UserAgent != userAgent)
            //{
            //    throw new UnauthorizedAccessException("Invalid or mismatched session.");
            //}

            // All good -> generate tokens
            //var newAccessToken = GenerateJwtToken(session.Email);
            var newAccessToken = GenerateJwtToken("some email");
            var newRefreshToken = Guid.NewGuid().ToString("N");

            //// Save new session
            //await _dapr.InvokeMethodAsync("auth-store", "store-refresh-token", new
            //{
            //    Email = session.Email,
            //    RefreshToken = newRefreshToken,
            //    Fingerprint = fingerprint,
            //    IP = ip,
            //    UserAgent = userAgent
            //});

            //// Delete old session
            //await _dapr.InvokeMethodAsync("auth-store", $"delete-refresh-token/{oldRefreshToken}");

            return (newAccessToken, newRefreshToken);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Token refresh failed");
            throw new UnauthorizedAccessException("Token refresh failed. Please log in again.", ex);
        }
    }

    public async Task LogoutAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var refreshToken = request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                // Delete session from accessor
                //await _dapr.InvokeMethodAsync(HttpMethod.Delete, "auth-store", $"delete-refresh-token/{refreshToken}");

                // For now just call unrelated method in the accessor
                var session = await _dapr.InvokeMethodAsync<TaskModel?>(
                                    HttpMethod.Get,
                                    "accessor",
                                    $"task/2",
                                    cancellationToken
                                );
            }

            // If the cookie doesn't exist, no need to throw — just silently succeed
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Logout failed");
            throw new UnauthorizedAccessException("Logout failed. Please try again.", ex);
        }
    }

    private string GenerateJwtToken(string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: new[] { new Claim(ClaimTypes.Name, email) },
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token).Trim();
    }
}