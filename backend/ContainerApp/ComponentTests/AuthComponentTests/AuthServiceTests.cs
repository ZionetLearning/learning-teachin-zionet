using Dapr.Client;
using Manager.Constants;
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

[Collection("Auth Test Collection")]
public class AuthServiceTests
{
    private readonly AuthTestFixture _fixture;

    public AuthServiceTests(AuthTestFixture fixture)
    {
        _fixture = fixture;
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

        _fixture.DaprMock
            .Setup(d => d.InvokeMethodAsync<LoginRequest, Guid?>(
                HttpMethod.Post,
                "accessor",
                "auth/login",
                It.IsAny<LoginRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _fixture.DaprMock
            .Setup(d => d.InvokeMethodAsync(
                HttpMethod.Post,
                "accessor",
                "api/refresh-sessions",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()));

        // Act
        var (accessToken, refreshToken) = await _fixture.AuthService.LoginAsync(request, _fixture.HttpContext.Request, CancellationToken.None);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshToken));
    }


    [Fact(DisplayName = "LoginAsync throws when user is null")]
    public async Task LoginAsync_Throws_WhenUserIdIsNull()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "wrong@email.com",
            Password = "bad"
        };

        _fixture.DaprMock
            .Setup(d => d.InvokeMethodAsync<LoginRequest, Guid?>(
                HttpMethod.Post,
                "accessor",
                "auth/login",
                It.IsAny<LoginRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null); // simulate failure

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _fixture.AuthService.LoginAsync(request, _fixture.HttpContext.Request, CancellationToken.None));

        Assert.Equal("Login failed. Please check your credentials.", ex.Message);
    }



}
