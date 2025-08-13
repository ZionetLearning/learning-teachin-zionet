using Microsoft.SemanticKernel;
using Polly;

namespace Engine;

public interface IRetryPolicy
{
    IAsyncPolicy<HttpResponseMessage> CreateHttpPolicy(ILogger logger);
    IAsyncPolicy<ChatMessageContent> CreateKernelPolicy(ILogger logger);
}

public class RetryPolicy : IRetryPolicy
{
    public IAsyncPolicy<HttpResponseMessage> CreateHttpPolicy(ILogger logger)
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult(msg => (int)msg.StatusCode == 408)
            .OrResult(msg => (int)msg.StatusCode == 429)
            .OrResult(msg => (int)msg.StatusCode is >= 500 and < 600)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetryAsync: async (outcome, delay, attempt, _) =>
                {
                    if (outcome.Exception is not null)
                    {
                        logger.LogWarning(outcome.Exception, "HTTP retry {RetryAttempt} after {Delay}", attempt, delay);
                    }
                    else
                    {
                        logger.LogWarning("HTTP retry {RetryAttempt} for status {StatusCode} after {Delay}",
                                           attempt, outcome.Result.StatusCode, delay);
                    }

                    await Task.CompletedTask;
                }
            );
    }
    public IAsyncPolicy<ChatMessageContent> CreateKernelPolicy(ILogger logger)
    {
        return Policy<ChatMessageContent>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<IOException>()
            .Or<TimeoutException>()
            .OrResult(result => result == null || string.IsNullOrWhiteSpace(result.Content))
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, attempt)) +
                    TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500)),
                onRetryAsync: async (outcome, delay, attempt, _) =>
                {
                    if (outcome.Exception is not null)
                    {
                        logger.LogWarning(outcome.Exception,
                            "Kernel retry {Attempt} after {Delay}", attempt, delay);
                    }
                    else
                    {
                        logger.LogWarning("Kernel retry {Attempt} — empty or null content after {Delay}",
                            attempt, delay);
                    }

                    await Task.CompletedTask;
                });
    }
}
