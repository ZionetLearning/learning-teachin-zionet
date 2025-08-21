using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Dapr.Client;
using Manager.Models.Auth;
using Manager.Services;
using Moq;
using System.Net;

namespace AuthComponentTests;

public class AuthTestFixture
{
    public JwtSettings JwtSettings { get; }
    public HttpContext HttpContext { get; }
    public Mock<DaprClient> DaprMock { get; }
    public IAuthService AuthService { get; }

    public AuthTestFixture()
    {
        // Load configuration from appsettings.local.json
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.local.json", optional: false)
            .Build();

        JwtSettings = config.GetSection("JwtSettings").Get<JwtSettings>()
                      ?? throw new InvalidOperationException("Missing JwtSettings");

        // Setup HttpContext
        HttpContext = new DefaultHttpContext();
        HttpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        HttpContext.Request.Headers["User-Agent"] = "Chrome Test";
        HttpContext.Request.Headers["x-fingerprint"] = "abc123";

        // Dapr mock
        DaprMock = new Mock<DaprClient>();

        // AuthService under test
        AuthService = new AuthService(DaprMock.Object, NullLogger<AuthService>.Instance, Options.Create(JwtSettings));
    }
}
