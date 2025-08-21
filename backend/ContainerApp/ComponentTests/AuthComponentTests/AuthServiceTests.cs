using Dapr.Client;
using Manager.Models.Auth;
using Manager.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace AuthComponentTests;

public class AuthServiceTests
{
    private readonly IAuthService _sut;
    private readonly Mock<DaprClient> _daprMock = new();
    private readonly HttpContext _httpContext;
    private readonly JwtSettings _jwtSettings = new()
    {
        Secret = "super-secret-key-that-is-32-bytes!",
        Issuer = "test-issuer",
        Audience = "test-audience",
        AccessTokenTTL = 15,
        RefreshTokenTTL = 7,
        RefreshTokenHashKey = "test-hash-key"
    };

    public AuthServiceTests()
    {
        _httpContext = new DefaultHttpContext();
        _httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        _httpContext.Request.Headers["User-Agent"] = "Chrome Test";
        _httpContext.Request.Headers["x-fingerprint"] = "abc123";

        var options = Options.Create(_jwtSettings);

        _sut = new AuthService(_daprMock.Object, NullLogger<AuthService>.Instance, options);
    }

    [Fact(DisplayName = "LoginAsync returns access and refresh token")]
    public async Task LoginAsync_ShouldReturnTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@email.com",
            Password = "pass"
        };

        _daprMock
            .Setup(d => d.InvokeMethodAsync<LoginRequest, Guid?>(
                HttpMethod.Post,
                "accessor",
                "auth/login",
                It.IsAny<LoginRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _daprMock
            .Setup(d => d.InvokeMethodAsync(
                HttpMethod.Post,
                "accessor",
                "api/refresh-sessions",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()));

        // Act
        var (accessToken, refreshToken) = await _sut.LoginAsync(request, _httpContext.Request, CancellationToken.None);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshToken));
    }
}
