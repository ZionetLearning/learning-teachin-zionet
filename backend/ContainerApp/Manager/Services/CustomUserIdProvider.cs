using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Manager.Services;

public sealed class CustomUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var principal = connection.User;
        if (principal is null)
        {
            return null;
        }

        var id = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? principal.Identity?.Name;

        return Guid.TryParse(id, out _) ? id : null;
    }
}