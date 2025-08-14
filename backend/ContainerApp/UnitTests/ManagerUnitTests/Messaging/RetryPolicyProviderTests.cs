using System.Net;
using FluentAssertions;
using Manager.Messaging;
using Microsoft.Extensions.Logging;
using Moq;

namespace Manager.UnitTests.Messaging;

public class RetryPolicyProviderTests
{
    private static RetryPolicyProvider Create(out Mock<ILogger> log)
    {
        log = new Mock<ILogger>();
        return new RetryPolicyProvider();
    }

    [Fact]
    public async Task Create_Retries_On_RetryableException_And_Stops_On_Success()
    {
        // Arrange
        var provider = Create(out var log);
        var settings = new QueueSettings { MaxRetryAttempts = 3, RetryDelaySeconds = 0 };
        var policy = provider.Create(settings, log.Object);

        int attempts = 0;

        // Act
        await policy.ExecuteAsync(() =>
        {
            attempts++;
            if (attempts < 3)
                throw new RetryableException("try again");
            return Task.CompletedTask;
        });

        // Assert
        attempts.Should().Be(3);
    }

    [Fact]
    public async Task Create_Does_Not_Retry_On_NonRetryableException()
    {
        var provider = Create(out var log);
        var settings = new QueueSettings { MaxRetryAttempts = 5, RetryDelaySeconds = 0 };
        var policy = provider.Create(settings, log.Object);

        int attempts = 0;
        Func<Task> act = () => policy.ExecuteAsync(() =>
        {
            attempts++;
            throw new NonRetryableException("do not retry");
        });

        await act.Should().ThrowAsync<NonRetryableException>();
        attempts.Should().Be(1);
    }

    [Fact]
    public async Task CreateHttpPolicy_Retries_On_429_And_500_Then_Succeeds()
    {
        var provider = Create(out var log);
        var policy = provider.CreateHttpPolicy(log.Object);

        int attempts = 0;

        var result = await policy.ExecuteAsync(async () =>
        {
            await Task.Yield();
            attempts++;
            return attempts switch
            {
                1 => new HttpResponseMessage((HttpStatusCode)429),
                2 => new HttpResponseMessage(HttpStatusCode.InternalServerError),
                _ => new HttpResponseMessage(HttpStatusCode.OK)
            };
        });


        attempts.Should().Be(3);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateHttpPolicy_Does_Not_Retry_On_200()
    {
        var provider = Create(out var log);
        var policy = provider.CreateHttpPolicy(log.Object);

        int attempts = 0;

        var result = await policy.ExecuteAsync(() =>
        {
            attempts++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });


        attempts.Should().Be(1);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateHttpPolicy_Retries_On_HttpRequestException()
    {
        var provider = Create(out var log);
        var policy = provider.CreateHttpPolicy(log.Object);

        int attempts = 0;
        await policy.Invoking(p => p.ExecuteAsync(() =>
        {
            attempts++;
            if (attempts < 2) throw new HttpRequestException("network");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        })).Should().NotThrowAsync();

        attempts.Should().Be(2);
    }
}
