using Polly;

namespace Accessor.Services;

public class RetryPolicy : IRetryPolicy
{
    public IAsyncPolicy<HttpResponseMessage> CreateHttpPolicy(ILogger logger)
    {
        // Exponential backoff; retries on transient HTTP errors/statuses
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult(msg => (int)msg.StatusCode == 408) // Request Timeout
            .OrResult(msg => (int)msg.StatusCode == 429) // Too Many Requests
            .OrResult(msg => (int)msg.StatusCode is >= 500 and < 600) // 5xx
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetryAsync: async (outcome, delay, attempt, _) =>
                {
                    if (outcome.Exception is not null)
                    {
                        logger.LogWarning(outcome.Exception, "HTTP retry {Attempt} after {Delay}", attempt, delay);
                    }
                    else
                    {
                        logger.LogWarning("HTTP retry {Attempt} for status {Status} after {Delay}",
                            attempt, outcome.Result.StatusCode, delay);
                    }

                    await Task.CompletedTask;
                });
    }
}
