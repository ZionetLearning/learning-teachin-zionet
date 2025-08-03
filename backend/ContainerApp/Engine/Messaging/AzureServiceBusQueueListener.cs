using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Engine.Messaging;

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
        this._settings = settings;
        this._retryPolicyProvider = retryPolicyProvider;
        this._logger = logger;

        this._processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = settings.MaxConcurrentCalls,
            PrefetchCount = settings.PrefetchCount,
            AutoCompleteMessages = false
        });
    }

    public async Task StartAsync(Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken)
    {
        var retryPolicy = this._retryPolicyProvider.Create(this._settings, this._logger);

        this._processor.ProcessMessageAsync += async args =>
        {
            var now = DateTimeOffset.UtcNow;
            var lockedUntil = args.Message.LockedUntil;
            var lockTimeout = lockedUntil - now;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linkedCts.CancelAfter(lockTimeout);
            try
            {
                var msg = JsonSerializer.Deserialize<T>(args.Message.Body);
                if (msg == null)
                {
                    this._logger.LogWarning("Failed to deserialize message.");
                    await args.DeadLetterMessageAsync(args.Message, cancellationToken: cancellationToken);
                    return;
                }

                await retryPolicy.ExecuteAsync(async () =>
                {
                    await handler(msg, linkedCts.Token);

                    if (this._settings.ProcessingDelayMs > 0)
                    {
                        await Task.Delay(this._settings.ProcessingDelayMs, linkedCts.Token);
                    }
                });

                await args.CompleteMessageAsync(args.Message, cancellationToken);
            }
            catch (RetryableException rex)
            {
                this._logger.LogWarning(rex, "Retryable error. Abandoning message.");
                await args.AbandonMessageAsync(args.Message, cancellationToken: cancellationToken);
            }
            catch (NonRetryableException nex)
            {
                this._logger.LogWarning(nex, "Non-retryable error. Dead-lettering message.");
                await args.DeadLetterMessageAsync(args.Message, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException) when (linkedCts.IsCancellationRequested)
            {
                this._logger.LogWarning("Handler exceeded lock duration. Abandoning message.");
                await args.AbandonMessageAsync(args.Message, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Unhandled error. Treating as retryable.");
                await args.AbandonMessageAsync(args.Message, cancellationToken: cancellationToken);
            }
        };

        this._processor.ProcessErrorAsync += args =>
        {
            this._logger.LogError(args.Exception, "Message handler error");
            return Task.CompletedTask;
        };

        await this._processor.StartProcessingAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await this._processor.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}

