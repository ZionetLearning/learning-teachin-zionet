using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Manager;

public sealed class CustomUserIdProvider : IUserIdProvider
{
    private readonly ILogger<CustomUserIdProvider> _logger;

    public CustomUserIdProvider(ILogger<CustomUserIdProvider> logger)
    {
        _logger = logger;
    }

    public string? GetUserId(HubConnectionContext connection)
    {
        var principal = connection.User;
        if (principal is null)
        {
            _logger.LogWarning("HubConnectionContext.User is null. Cannot resolve user id.");
            return null;
        }

        var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? principal.Identity?.Name;

        if (!Guid.TryParse(id, out _))
        {
            _logger.LogWarning("User id '{Id}' is not a valid GUID.", id);
            return null;
        }

        return id;
    }
}