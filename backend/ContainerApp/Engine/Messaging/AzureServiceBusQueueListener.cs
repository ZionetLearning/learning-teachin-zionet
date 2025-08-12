using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System.Text.Json;

namespace Engine.Messaging;

public class AzureServiceBusQueueListener<T> : IQueueListener<T>, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ServiceBusProcessor _processor;
    private readonly ServiceBusAdministrationClient _admin;
    private readonly string _queueName;
    private readonly IRetryPolicyProvider _retryPolicyProvider;
    private readonly QueueSettings _settings;
    private readonly ILogger<AzureServiceBusQueueListener<T>> _logger;

    public AzureServiceBusQueueListener(
        ServiceBusClient client,
        ServiceBusAdministrationClient admin,
        string queueName,
        QueueSettings settings,
        IRetryPolicyProvider retryPolicyProvider,
        ILogger<AzureServiceBusQueueListener<T>> logger)
    {
        _admin = admin;
        _queueName = queueName;
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

    public async Task StartAsync(Func<T, Func<Task>, CancellationToken, Task> handler, CancellationToken cancellationToken)
    {
        // Wait for emulator to finish creating the queue
        var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(60);
        var delay = TimeSpan.FromMilliseconds(500);

        while (DateTimeOffset.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (await _admin.QueueExistsAsync(_queueName, cancellationToken))
                {
                    _logger.LogInformation("Queue '{Queue}' is available.", _queueName);
                    break;
                }

                _logger.LogInformation("Queue '{Queue}' not ready yet. Waiting...", _queueName);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Queue existence check failed; retrying.");
            }

            await Task.Delay(delay, cancellationToken);
            if (delay < TimeSpan.FromSeconds(5))
            {
                delay += delay; // simple backoff
            }
        }

        var retryPolicy = _retryPolicyProvider.Create(_settings, _logger);

        _processor.ProcessMessageAsync += async args =>
        {
            var lockTimeout = args.Message.LockedUntil - DateTimeOffset.UtcNow;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (lockTimeout > TimeSpan.Zero)
            {
                linkedCts.CancelAfter(lockTimeout);
            }

            var renewLock = async () =>
            {
                await args.RenewMessageLockAsync(args.Message, linkedCts.Token);
                _logger.LogDebug("Lock renewed for message {MessageId}", args.Message.MessageId);
            };

            try
            {
                var json = args.Message.Body.ToString();
                _logger.LogDebug("Raw message body: {Json}", json);

                var msg = JsonSerializer.Deserialize<T>(json, JsonOptions);
                if (msg == null)
                {
                    _logger.LogWarning("Failed to deserialize message, dead-lettering.");
                    await args.DeadLetterMessageAsync(args.Message, cancellationToken: cancellationToken);
                    return;
                }

                await retryPolicy.ExecuteAsync(async () =>
                {
                    await handler(msg, renewLock, linkedCts.Token);

                    if (_settings.ProcessingDelayMs > 0)
                    {
                        await Task.Delay(_settings.ProcessingDelayMs, linkedCts.Token);
                    }
                });

                await args.CompleteMessageAsync(args.Message, cancellationToken);
            }
            catch (RetryableException rex)
            {
                _logger.LogWarning(rex, "Retryable error. Abandoning message.");
                await args.AbandonMessageAsync(args.Message, cancellationToken: cancellationToken);
            }
            catch (NonRetryableException nex)
            {
                _logger.LogWarning(nex, "Non-retryable error. Dead-lettering message.");
                await args.DeadLetterMessageAsync(args.Message, cancellationToken: cancellationToken);
            }
            catch (OperationCanceledException) when (linkedCts.IsCancellationRequested)
            {
                _logger.LogWarning("Handler exceeded lock duration. Abandoning message.");
                await args.AbandonMessageAsync(args.Message, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error. Treating as retryable.");
                await args.AbandonMessageAsync(args.Message, cancellationToken: cancellationToken);
            }
        };

        _processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception, "Message handler error");
            return Task.CompletedTask;
        };

        await _processor.StartProcessingAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _processor.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
