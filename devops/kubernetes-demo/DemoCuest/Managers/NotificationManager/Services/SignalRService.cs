using Microsoft.Azure.SignalR.Management;

namespace NotificationManager.Services;

public class SignalRService(IConfiguration configuration, ILoggerFactory loggerFactory)
    : IHostedService, IHubContextStore
{
    private const string CallbackHub = "todohub";

    public ServiceHubContext? TodoNotificationsHubContext { get; private set; }

    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        using var serviceManager = new ServiceManagerBuilder()
            .WithConfiguration(configuration)
            .WithLoggerFactory(loggerFactory)
            .BuildServiceManager();
        TodoNotificationsHubContext = await serviceManager.CreateHubContextAsync(CallbackHub, cancellationToken);
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        if (TodoNotificationsHubContext != null)
        {
            return TodoNotificationsHubContext.DisposeAsync();
        }
        return Task.CompletedTask;
    }
}
