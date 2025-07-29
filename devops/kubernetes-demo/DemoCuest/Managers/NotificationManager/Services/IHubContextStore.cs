using Microsoft.Azure.SignalR.Management;

namespace NotificationManager.Services;

public interface IHubContextStore
{
    public ServiceHubContext? TodoNotificationsHubContext { get; }
}

