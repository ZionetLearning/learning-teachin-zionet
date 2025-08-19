using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapr.Client;
using Manager.Models.Auth;
using Manager.Models.Auth.RefreshSessions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Manager.Services;

public class AuthService : IAuthService
{
    private readonly DaprClient _dapr;
    private readonly ILogger<AuthService> _log;
    private readonly JwtSettings _jwt;

    // To DO: get the user ID from the users table
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
            var UserId = await _dapr.InvokeMethodAsync<LoginRequest, Guid?>(
                HttpMethod.Post,
                "accessor",
                "auth/login",
                loginRequest,
                cancellationToken
            );

            if (UserId is null)
            {
                _log.LogError("Login failed for user {Email}", loginRequest.Email);
                throw new UnauthorizedAccessException("Login failed. Please check your credentials.");
            }

            // Generate access and refresh tokens
            var accessToken = GenerateJwtToken(loginRequest.Email);
            var refreshToken = Guid.NewGuid().ToString("N");

            var refreshHash = HashRefreshToken(refreshToken, _jwt.RefreshTokenHashKey);

            // Collect session fingerprint data
            var fingerprint = httpRequest.Headers["x-fingerprint"].ToString();
            var fingerprintHash = string.IsNullOrWhiteSpace(fingerprint)
            ? null
            : HashRefreshToken(fingerprint, _jwt.RefreshTokenHashKey);

            var ua = httpRequest.Headers["User-Agent"].ToString();
            if (string.IsNullOrWhiteSpace(ua))
            {
                ua = "unknown";
            }

            var ip = httpRequest.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // The user Id is taken from the DB, with the credantials of email and password
            var session = new RefreshSessionRequest
            {
                UserId = Guid.NewGuid(),
                RefreshTokenHash = refreshHash,
                DeviceFingerprintHash = fingerprintHash,
                IP = ip,
                UserAgent = ua,
            };

            await _dapr.InvokeMethodAsync(
                HttpMethod.Post,
                "accessor",
                "api/refresh-sessions",
                session,
                cancellationToken);

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
            var oldRefreshToken = request.Cookies["refreshToken"]
            ?? throw new UnauthorizedAccessException("Missing refresh token.");

            // Collect session fingerprint data
            var fingerprint = request.Headers["x-fingerprint"].ToString() ?? "unknown";
            var userAgent = request.Headers["User-Agent"].ToString() ?? "unknown";
            var ip = request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var oldHash = HashRefreshToken(oldRefreshToken, _jwt.RefreshTokenHashKey);

            // Get session from Accessor
            var session = await _dapr.InvokeMethodAsync<RefreshSessionDto>(
                HttpMethod.Get,
                "accessor",
                $"api/refresh-sessions/by-token-hash/{oldHash}",
                cancellationToken
            ) ?? throw new UnauthorizedAccessException("Invalid or mismatched session.");

            if (session.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh session expired.");
            }

            if (!string.IsNullOrWhiteSpace(fingerprint) && !string.IsNullOrWhiteSpace(session.DeviceFingerprintHash))
            {
                var fpHash = HashRefreshToken(fingerprint, _jwt.RefreshTokenHashKey);
                if (!SlowEquals(fpHash, session.DeviceFingerprintHash))
                {
                    throw new UnauthorizedAccessException("Device fingerprint mismatch.");
                }
            }

            // IP & UA checks
            if (!IpRoughMatch(session.IP, ip))
            {
                _log.LogWarning("IP mismatch for session {SessionId}. Saved={Saved} Current={Current}",
                    session.Id, session.IP, ip);
            }

            if (!UserAgentRoughMatch(session.UserAgent, userAgent))
            {
                _log.LogWarning("UA mismatch for session {SessionId}. Saved={Saved} Current={Current}",
                    session.Id, session.UserAgent, userAgent);
            }

            // All good -> generate tokens

            var newAccessToken = GenerateJwtToken(session.UserId.ToString());
            var newRefreshToken = Guid.NewGuid().ToString("N");
            var newRefreshHash = HashRefreshToken(newRefreshToken, _jwt.RefreshTokenHashKey);

            var now = DateTimeOffset.UtcNow;

            var rotatePayload = new RotateRefreshSessionRequest
            {
                NewRefreshTokenHash = newRefreshHash,
                NewExpiresAt = now.AddDays(_jwt.RefreshTokenTTL),
                LastSeenAt = now,
                IssuedAt = now
            };

            // Save new session
            await _dapr.InvokeMethodAsync(
                HttpMethod.Put,
                "accessor",
                $"api/refresh-sessions/{session.Id}/rotate",
                rotatePayload,
                cancellationToken);

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
            if (string.IsNullOrEmpty(refreshToken))
            {
                _log.LogInformation("No refresh token cookie. Nothing to logout.");
                return;
            }

            var hash = HashRefreshToken(refreshToken, _jwt.RefreshTokenHashKey);

            // Lookup session by hash
            var session = await _dapr.InvokeMethodAsync<RefreshSessionDto?>(
                HttpMethod.Get, "accessor", $"api/refresh-sessions/by-token-hash/{hash}", cancellationToken);

            if (session is null)
            {
                _log.LogInformation("No session found for given refresh token. Already logged out?");
                return;
            }

            // Delete by sessionId
            await _dapr.InvokeMethodAsync(
                HttpMethod.Delete, "accessor", $"api/refresh-sessions/{session.Id}", cancellationToken);

            _log.LogInformation("Deleted session {SessionId}", session.Id);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Logout failed");
            throw new UnauthorizedAccessException("Logout failed. Please try again.", ex);
        }
    }

    #region Helpers 
    private string GenerateJwtToken(string userId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: new[] { new Claim(ClaimTypes.Name, userId) },
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenTTL),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token).Trim();
    }

    private static string HashRefreshToken(string token, string key)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(key));
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes); // uppercase hex
    }

    private static bool IpRoughMatch(string saved, string current)
    {
        static string norm(string ip) => ip.StartsWith("::ffff:", StringComparison.OrdinalIgnoreCase)
            ? ip.Substring("::ffff:".Length)
            : ip;
        saved = norm(saved ?? "");
        current = norm(current ?? "");
        if (string.IsNullOrEmpty(saved) || string.IsNullOrEmpty(current))
        {
            return true;
        }

        return saved == current;
    }

    private static bool UserAgentRoughMatch(string a, string b)
    {
        if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b))
        {
            return true;
        }

        a = a.ToLowerInvariant();
        b = b.ToLowerInvariant();
        // very light check
        return a.Contains("chrome") == b.Contains("chrome")
            && a.Contains("safari") == b.Contains("safari")
            && a.Contains("mobile") == b.Contains("mobile");
    }

    private static bool SlowEquals(string x, string y)
    {
        // constant-time compare to avoid timing leaks
        if (x.Length != y.Length)
        {
            return false;
        }

        var result = 0;
        for (var i = 0; i < x.Length; i++)
        {
            result |= x[i] ^ y[i];
        }

        return result == 0;
    }

    #endregion
}