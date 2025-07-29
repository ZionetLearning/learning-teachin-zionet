using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Engine.Messaging
{
    public class AzureServiceBusQueueListener<T> : IQueueListener<T>, IAsyncDisposable
    {
        private readonly ServiceBusProcessor _processor;
        private readonly IRetryPolicyProvider _retryPolicyProvider;
        private readonly QueueSettings _settings;
        private readonly ILogger<AzureServiceBusQueueListener<T>> _logger;

        public AzureServiceBusQueueListener(
            ServiceBusClient client,
            string queueName,
            QueueSettings settings,
            IRetryPolicyProvider retryPolicyProvider,
            ILogger<AzureServiceBusQueueListener<T>> logger)
        {
            _settings = settings;
            _retryPolicyProvider = retryPolicyProvider;
            _logger = logger;

            _processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = settings.MaxConcurrentCalls,
                PrefetchCount = settings.PrefetchCount,
                AutoCompleteMessages = false
            });
        }

        public async Task StartAsync(Func<T, CancellationToken, Task> handler, CancellationToken ct)
        {
            var retryPolicy = _retryPolicyProvider.Create(_settings, _logger);

            _processor.ProcessMessageAsync += async args =>
            {
                try
                {
                    var msg = JsonSerializer.Deserialize<T>(args.Message.Body);
                    if (msg == null)
                    {
                        _logger.LogWarning("Failed to deserialize message.");
                        await args.DeadLetterMessageAsync(args.Message, cancellationToken: ct);
                        return;
                    }

                    await retryPolicy.ExecuteAsync(async () =>
                    {
                        await handler(msg, ct);

                        if (_settings.ProcessingDelayMs > 0)
                            await Task.Delay(_settings.ProcessingDelayMs, ct);
                    });

                    await args.CompleteMessageAsync(args.Message, ct);
                }
                catch (RetryableException rex)
                {
                    _logger.LogWarning(rex, "Retryable error. Abandoning message.");
                    await args.AbandonMessageAsync(args.Message, cancellationToken: ct);
                }
                catch (NonRetryableException nex)
                {
                    _logger.LogWarning(nex, "Non-retryable error. Dead-lettering message.");
                    await args.DeadLetterMessageAsync(args.Message, cancellationToken: ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error. Treating as retryable.");
                    await args.AbandonMessageAsync(args.Message, cancellationToken: ct);
                }
            };

            _processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, "Message handler error");
                return Task.CompletedTask;
            };

            await _processor.StartProcessingAsync(ct);
        }

        public async ValueTask DisposeAsync() => await _processor.DisposeAsync();
    }

}

