using Manager.Constants;
using Manager.Models.Users;
using Manager.Services;
using Manager.Services.Clients.Accessor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Manager.Hubs;

[Authorize]
public class NotificationHub : Hub<INotificationClient>
{
    private readonly ILogger<NotificationHub> _logger;
    private readonly IOnlinePresenceService _presence;
    private readonly IAccessorClient _accessorClient;

    public NotificationHub(ILogger<NotificationHub> logger, IOnlinePresenceService presence, IAccessorClient accessorClient)
    {
        _logger = logger;
        _presence = presence;
        _accessorClient = accessorClient;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            _logger.LogInformation(
                "Conn={Conn} UserIdentifier={UserId} Name={Name}",
                Context.ConnectionId, Context.UserIdentifier, Context.User?.Identity?.Name);
            if (Context.UserIdentifier != null)
            {
                var user = await _accessorClient.GetUserAsync(Guid.Parse(Context.UserIdentifier)).ConfigureAwait(false);
                if (user != null)
                {
                    var userId = user.UserId.ToString();
                    var name = user.FirstName + " " + user.LastName;
                    var role = user.Role.ToString();
                    if (user.Role == Role.Admin)
                    {
                        return;
                    }

                    var (first, count) = await _presence.AddConnectionAsync(userId, name, role, Context.ConnectionId);
                    if (first)
                    {
                        await Clients.Group(AdminGroups.Admins).UserOnline(userId, role, name);
                    }
                    else
                    {
                        await Clients.Group(AdminGroups.Admins).UpdateUserConnections(userId, count);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OnConnectedAsync for connection {Conn}", Context.ConnectionId);
        }
        finally
        {
            await base.OnConnectedAsync();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);

            if (Context.UserIdentifier != null)
            {
                var user = await _accessorClient.GetUserAsync(Guid.Parse(Context.UserIdentifier)).ConfigureAwait(false);
                if (user != null)
                {
                    var userId = user.UserId.ToString();
                    if (user.Role == Role.Admin)
                    {
                        return;
                    }

                    var (last, count) = await _presence.RemoveConnectionAsync(userId, Context.ConnectionId);

                    if (last)
                    {
                        await Clients.Group(AdminGroups.Admins).UserOffline(userId);
                    }
                    else
                    {
                        await Clients.Group(AdminGroups.Admins).UpdateUserConnections(userId, count);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OnDisconnectedAsync for connection {Conn}", Context.ConnectionId);
        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
    [Authorize(PolicyNames.AdminOnly)]
    public Task SubscribeAdmin() =>
        Groups.AddToGroupAsync(Context.ConnectionId, AdminGroups.Admins);
    [Authorize(PolicyNames.AdminOnly)]
    public Task UnSubscribeAdmin() =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroups.Admins);
}
