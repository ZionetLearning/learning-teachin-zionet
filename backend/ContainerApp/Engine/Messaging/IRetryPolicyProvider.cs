using Polly;

namespace Engine.Messaging
{
    public interface IRetryPolicyProvider
    {
        IAsyncPolicy Create(QueueSettings settings, ILogger logger);
        IAsyncPolicy<HttpResponseMessage> CreateHttpPolicy(ILogger logger);
    }

    public class RetryPolicyProvider : IRetryPolicyProvider
    {
        public IAsyncPolicy Create(QueueSettings settings, ILogger logger)
        {
            return Policy
                .Handle<Exception>(ShouldRetry)
                .WaitAndRetryAsync(
                    retryCount: settings.MaxRetryAttempts,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(settings.RetryDelaySeconds),
                    onRetry: (exception, delay, retryAttempt, _) =>
                    {
                        logger.LogWarning(exception, "Retry {RetryAttempt} in {Delay}", retryAttempt, delay);
                    });
        }

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
                            logger.LogWarning(outcome.Exception, "HTTP retry {RetryAttempt} after {Delay}", attempt, delay);
                        else
                            logger.LogWarning("HTTP retry {RetryAttempt} for status {StatusCode} after {Delay}",
                                               attempt, outcome.Result.StatusCode, delay);
                        await Task.CompletedTask;
                    }
                );
        }

        private bool ShouldRetry(Exception ex)
        {
            return ex switch
            {
                RetryableException => true,
                NonRetryableException => false,
                _ => false
            };
        }
    }

}
