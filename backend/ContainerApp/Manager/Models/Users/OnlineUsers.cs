namespace Manager.Models.Users;

public sealed record OnlineUserDto(string UserId, string Name, string Role, int ConnectionsCount);

public sealed record UserMeta(string Name, string Role);

public static class PresenceKeys
{
    public static string All => "presence:all";
    public static string Conns(string userId) => $"presence:{userId}:conns";
    public static string Meta(string userId) => $"presence:{userId}:meta";
}

public static class AdminGroups
{
    public const string Admins = "Admins";
}