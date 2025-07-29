using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace Engine.Messaging
{
    public class AzureServiceBusQueueListener<T> : IQueueListener<T>, IAsyncDisposable
    {
        readonly ServiceBusProcessor _processor;
        public AzureServiceBusQueueListener(ServiceBusClient client, string queueName)
        {
            _processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());
        }
        public async Task StartAsync(Func<T, CancellationToken, Task> handler, CancellationToken ct)
        {
            _processor.ProcessMessageAsync += async args =>
            {
                var msg = JsonSerializer.Deserialize<T>(args.Message.Body);
                await handler(msg!, ct);
                await args.CompleteMessageAsync(args.Message, ct);
            };
            _processor.ProcessErrorAsync += _ => Task.CompletedTask;
            await _processor.StartProcessingAsync(ct);
        }
        public async ValueTask DisposeAsync() => await _processor.DisposeAsync();
    }
}

