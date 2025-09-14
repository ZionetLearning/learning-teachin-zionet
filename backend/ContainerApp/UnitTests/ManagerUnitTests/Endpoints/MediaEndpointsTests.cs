using System.Net;
using FluentAssertions;
using Manager.Endpoints;
using Manager.Services.Clients.Accessor;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ManagerUnitTests.Endpoints;

public class MediaEndpointsTests
{
    private readonly Mock<IAccessorClient> _accessorClient = new(MockBehavior.Strict);

    private static async Task<(int status, string body)> ExecuteAsync(IResult result)
    {
        var ctx = new DefaultHttpContext();
        // Minimal DI container for ProblemDetails writer and other features used by IResult implementations
        var services = new ServiceCollection()
            .AddLogging()
            .AddRouting()
            .AddHttpContextAccessor()
            .BuildServiceProvider();
        ctx.RequestServices = services;

        var responseBody = new MemoryStream();
        ctx.Response.Body = responseBody;
        await result.ExecuteAsync(ctx);
        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(ctx.Response.Body);
        string body = await reader.ReadToEndAsync();
        return (ctx.Response.StatusCode, body);
    }

    [Fact]
    public async Task GetSpeechToken_ReturnsOk_WithWrappedToken()
    {
        var speechTokenResponse = new SpeechTokenResponse { Token = "abc123", Region = "eastus" };
        
        _accessorClient.Reset();
        _accessorClient.Setup(a => a.GetSpeechTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(speechTokenResponse);

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(MediaEndpoints),
            "GetSpeechTokenAsync",
            _accessorClient.Object
        );

        var (status, body) = await ExecuteAsync(result);
        status.Should().Be((int)HttpStatusCode.OK);
        body.Should().Contain("abc123");
        body.Should().Contain("token");
        body.Should().Contain("region");
        body.Should().Contain("eastus");
    }

    [Fact]
    public async Task GetSpeechToken_ReturnsProblem_WhenEmptyToken()
    {
        _accessorClient.Reset();
        _accessorClient.Setup(a => a.GetSpeechTokenAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("failure"));

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(MediaEndpoints),
            "GetSpeechTokenAsync",
            _accessorClient.Object
        );

        var (status, _) = await ExecuteAsync(result);
        status.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetSpeechToken_ReturnsProblem_OnException()
    {
        _accessorClient.Reset();
        _accessorClient.Setup(a => a.GetSpeechTokenAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("failure"));

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(MediaEndpoints),
            "GetSpeechTokenAsync",
            _accessorClient.Object
        );

        var (status, _) = await ExecuteAsync(result);
        status.Should().Be((int)HttpStatusCode.InternalServerError);
    }
}
