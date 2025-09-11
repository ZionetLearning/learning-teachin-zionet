using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Dapr.Client;
using Manager.Constants;
using Manager.Models.Auth;
using Manager.Models.Auth.RefreshSessions;
using Manager.Models.Users;
using Manager.Services.Clients.Accessor;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Manager.Services;

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _log;
    private readonly JwtSettings _jwt;
    private readonly IAccessorClient _accessorClient;

    public AuthService(ILogger<AuthService> log, IOptions<JwtSettings> jwtOptions, IAccessorClient accessorClient)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _jwt = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        _accessorClient = accessorClient;
    }

    public async Task<(string, string)> LoginAsync(LoginRequest loginRequest, HttpRequest httpRequest, CancellationToken cancellationToken)
    {
        _log.LogInformation("Login attempt for user {Email}", loginRequest.Email);
        try
        {
            _log.LogInformation("Calling accessor with timeout 30s...");
            var response = await _accessorClient.LoginUserAsync(loginRequest, cancellationToken.None);
            _log.LogInformation("Accessor call finished");

            if (response is null || response.UserId == Guid.Empty)
            {
                _log.LogError("Login failed for user {Email}", loginRequest.Email);
                throw new UnauthorizedAccessException($"Login failed for user {loginRequest.Email}");
            }

            // Generate access and refresh tokens
            var accessToken = GenerateJwtToken(response.UserId, response.Role);
            var refreshToken = Guid.NewGuid().ToString("N");

            var refreshHash = HashRefreshToken(refreshToken, _jwt.RefreshTokenHashKey);

            // Collect session fingerprint data
            var fingerprint = httpRequest.Headers["x-fingerprint"].ToString();
            var fingerprintHash = string.IsNullOrWhiteSpace(fingerprint)
            ? null
            : HashRefreshToken(fingerprint, _jwt.RefreshTokenHashKey);

            var ua = string.IsNullOrWhiteSpace(httpRequest.Headers.UserAgent)
                ? AuthSettings.UnknownIpFallback
                : httpRequest.Headers.UserAgent.ToString();

            var ip = httpRequest.HttpContext.Connection.RemoteIpAddress?.ToString() ?? AuthSettings.UnknownIpFallback;

            // The user Id is taken from the DB, with the credantials of email and password
            var session = new RefreshSessionRequest
            {
                UserId = response.UserId,
                RefreshTokenHash = refreshHash,
                DeviceFingerprintHash = fingerprintHash,
                IP = ip,
                UserAgent = ua,
            };

            await _accessorClient.SaveSessionDBAsync(session, cancellationToken);
            return (accessToken, refreshToken);
        }

        catch (InvocationException ex) when (ex.InnerException is HttpRequestException httpEx)
        {
            if (httpEx.StatusCode == HttpStatusCode.Unauthorized)
            {
                _log.LogError(ex, "Unauthorized response from accessor for {Email}", loginRequest.Email);
                throw new UnauthorizedAccessException("Login failed. Please check your credentials.", ex);
            }
            // rethrow other HTTP errors
            throw;
        }

        catch (UnauthorizedAccessException ex)
        {
            _log.LogError(ex, "Login failed for user {Email}, Authorized exception.", loginRequest.Email);
            throw new UnauthorizedAccessException(ex.Message, ex);
        }

        catch (Exception ex)
        {
            _log.LogError(ex, "Login failed for user {Email}, Internal exception.", loginRequest.Email);
            throw new Exception("Internal server error. Login failed.", ex);
        }
    }

    public async Task<(string accessToken, string refreshToken)> RefreshTokensAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        _log.LogInformation("Token refresh attempt");
        try
        {
            // Get the old refresh token from the request cookies
            var oldRefreshToken = request.Cookies[AuthSettings.RefreshTokenCookieName]
            ?? throw new UnauthorizedAccessException("Missing refresh token.");

            // TODO: In Future, when we will have domain or real frontend validate Origin and Referer headers

            // For now, just comment it because we check local so we dont have the access for csrf cookie
            //var csrfCookie = request.Cookies[AuthSettings.CsrfTokenCookieName];

            var csrfHeader = request.Headers["X-CSRF-Token"].ToString();

            //if (string.IsNullOrWhiteSpace(csrfHeader) || csrfCookie == null || !SlowEquals(csrfCookie, csrfHeader))
            if (string.IsNullOrWhiteSpace(csrfHeader))

            {
                throw new UnauthorizedAccessException("Invalid CSRF token");
            }

            // Collect session fingerprint data
            var fingerprint = request.Headers["x-fingerprint"].ToString() ?? AuthSettings.UnknownIpFallback;
            var userAgent = request.Headers.UserAgent.ToString() ?? AuthSettings.UnknownIpFallback;
            var ip = request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? AuthSettings.UnknownIpFallback;

            var oldHash = HashRefreshToken(oldRefreshToken, _jwt.RefreshTokenHashKey);

            // Get session from Accessor
            RefreshSessionDto session;
            try
            {
                session = await _accessorClient.GetSessionAsync(oldHash, cancellationToken)
                    ?? throw new UnauthorizedAccessException("Invalid or mismatched session.");
            }
            catch (InvocationException ex) when (ex.InnerException is HttpRequestException httpEx && httpEx.StatusCode == HttpStatusCode.NotFound)
            {
                _log.LogWarning("No session found for given refresh token hash.");
                throw new UnauthorizedAccessException("Invalid or mismatched session.", ex);
            }

            // ------------------------ strat validations ------------------------

            // Expiry Validate
            if (session.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh session expired.");
            }

            // Fingerprint Validate
            if (!string.IsNullOrWhiteSpace(fingerprint) && !string.IsNullOrWhiteSpace(session.DeviceFingerprintHash))
            {
                var fpHash = HashRefreshToken(fingerprint, _jwt.RefreshTokenHashKey);
                if (!SlowEquals(fpHash, session.DeviceFingerprintHash))
                {
                    throw new UnauthorizedAccessException("Device fingerprint mismatch.");
                }
            }

            // IP Validate
            if (!IpRoughMatch(session.IP, ip))
            {
                _log.LogWarning("IP mismatch for session {SessionId}. Saved={Saved} Current={Current}",
                    session.Id, session.IP, ip);
                throw new UnauthorizedAccessException("IP address mismatch.");
            }

            // User-Agent Validate
            if (!UserAgentRoughMatch(session.UserAgent, userAgent))
            {
                _log.LogWarning("UA mismatch for session {SessionId}. Saved={Saved} Current={Current}",
                    session.Id, session.IP, ip);
                throw new UnauthorizedAccessException("User-Agent mismatch.");
            }
            // get the role of the user
            var user = await _accessorClient.GetUserAsync(session.UserId)
                ?? throw new UnauthorizedAccessException("User not found.");
            var role = user.Role;

            // All good -> generate tokens

            var newAccessToken = GenerateJwtToken(session.UserId, role);
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

            // Update session using accessor client
            await _accessorClient.UpdateSessionDBAsync(session.Id, rotatePayload, cancellationToken);

            return (newAccessToken, newRefreshToken);
        }
        catch (UnauthorizedAccessException ex)
        {
            _log.LogError(ex, "Refresh token failed, Authorized exception.");
            throw;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Refresh token failed, Internal exception.");
            throw new Exception("Token refresh failed. Please log in again.", ex);
        }
    }

    public async Task LogoutAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var refreshToken = request.Cookies[AuthSettings.RefreshTokenCookieName];
            if (string.IsNullOrEmpty(refreshToken))
            {
                _log.LogInformation("No refresh token cookie. Nothing to logout.");
                return;
            }

            var hash = HashRefreshToken(refreshToken, _jwt.RefreshTokenHashKey);

            // Lookup session by hash
            var session = await _accessorClient.GetSessionAsync(hash, cancellationToken);

            if (session is null)
            {
                _log.LogInformation("No session found for given refresh token. Already logged out?");
                return;
            }

            // Delete by sessionId
            await _accessorClient.DeleteSessionDBAsync(session.Id, cancellationToken);

            _log.LogInformation("Deleted session {SessionId}", session.Id);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Logout failed");
            throw new UnauthorizedAccessException("Logout failed. Please try again.", ex);
        }
    }

    #region Helpers 
    private string GenerateJwtToken(Guid userId, Role role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(AuthSettings.UserIdClaimType, userId.ToString()), // userID 
            new Claim(AuthSettings.RoleClaimType, role.ToString()) // user role
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            // Store the userId and the role in the token
            claims: claims,
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