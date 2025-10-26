using System.Text.Json;
using Dapr;
using Dapr.Client;
using Manager.Constants;
using Manager.Models.Users;

namespace Manager.Services;

public sealed class OnlinePresenceService : IOnlinePresenceService
{
    private readonly DaprClient _dapr;
    private const string Store = AppIds.StateStore;
    private readonly ILogger<OnlinePresenceService> _logger;

    private static readonly StateOptions SafeWrite = new()
    {
        Consistency = ConsistencyMode.Strong,
        Concurrency = ConcurrencyMode.LastWrite
    };

    private static readonly StateOptions WeakWrite = new()
    {
        Consistency = ConsistencyMode.Eventual,
        Concurrency = ConcurrencyMode.LastWrite
    };

    private const int MaxAttempts = 8;

    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private static readonly IReadOnlyDictionary<string, string> TtlMeta = new Dictionary<string, string> { ["ttlInSeconds"] = "86400" };
    public OnlinePresenceService(DaprClient dapr, ILogger<OnlinePresenceService> logger)
    {
        _dapr = dapr;
        _logger = logger;
    }

    private static async Task<T> WithRetry<T>(Func<Task<T>> action, CancellationToken ct)
    {
        var rnd = new Random();
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await action();
            }
            catch (DaprException) when (attempt < MaxAttempts)
            {
                var delayMs = Math.Min(1000, (int)(Math.Pow(2, attempt) * 10 + rnd.Next(0, 25)));
                await Task.Delay(delayMs, ct);
            }
        }
    }

    public async Task<(bool first, int count)> AddConnectionAsync(string userId, string name, string role, string connectionId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(userId) ||
            string.IsNullOrEmpty(name) ||
            string.IsNullOrEmpty(role) ||
            string.IsNullOrEmpty(connectionId))
        {
            throw new ArgumentException("One or more arguments are missing");
        }

        var connsKey = PresenceKeys.Conns(userId);
        var metaKey = PresenceKeys.Meta(userId);
        var allKey = PresenceKeys.All;

        try
        {
            var firstConnection = false;
            var currentCount = 0;

            await WithRetry(async () =>
            {
                var connsEntry = await _dapr.GetStateEntryAsync<HashSet<string>>(Store, connsKey, cancellationToken: ct);
                var conns = connsEntry.Value ?? new HashSet<string>(StringComparer.Ordinal);
                var wasEmptyBefore = conns.Count == 0;
                var added = conns.Add(connectionId);
                if (!added)
                {
                    currentCount = conns.Count;
                    return 0;
                }

                connsEntry.Value = conns;
                await connsEntry.SaveAsync(SafeWrite, metadata: TtlMeta, cancellationToken: ct);

                currentCount = conns.Count;
                if (wasEmptyBefore)
                {
                    firstConnection = true;
                }

                return 0;
            }, ct);

            if (firstConnection)
            {
                await WithRetry(async () =>
                {
                    var metaEntry = await _dapr.GetStateEntryAsync<UserMeta>(Store, metaKey, cancellationToken: ct);
                    metaEntry.Value = new UserMeta(name, role);
                    await metaEntry.SaveAsync(SafeWrite, metadata: TtlMeta, cancellationToken: ct);
                    return 0;
                }, ct);

                await WithRetry(async () =>
                {
                    var allEntry = await _dapr.GetStateEntryAsync<HashSet<string>>(Store, allKey, cancellationToken: ct);
                    var all = allEntry.Value ?? new HashSet<string>(StringComparer.Ordinal);
                    all.Add(userId);
                    allEntry.Value = all;
                    await allEntry.SaveAsync(WeakWrite, metadata: TtlMeta, cancellationToken: ct);
                    return 0;
                }, ct);
            }

            return (firstConnection, currentCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding connection");
            throw;
        }
    }

    public async Task<(bool last, int count)> RemoveConnectionAsync(string userId, string connectionId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(connectionId))
        {
            throw new ArgumentException("One or more arguments are missing");
        }

        var connsKey = PresenceKeys.Conns(userId);
        var metaKey = PresenceKeys.Meta(userId);
        var allKey = PresenceKeys.All;

        try
        {
            var lastConnection = false;
            var currentCount = 0;

            await WithRetry(async () =>
            {
                var connsEntry = await _dapr.GetStateEntryAsync<HashSet<string>>(Store, connsKey, cancellationToken: ct);
                var conns = connsEntry.Value ?? new HashSet<string>(StringComparer.Ordinal);

                if (!conns.Contains(connectionId))
                {
                    currentCount = conns.Count;
                    return 0;
                }

                var wasLastBefore = conns.Count == 1;
                conns.Remove(connectionId);

                if (conns.Count == 0)
                {
                    await connsEntry.DeleteAsync(SafeWrite, cancellationToken: ct);
                    lastConnection = wasLastBefore;
                    currentCount = 0;
                }
                else
                {
                    connsEntry.Value = conns;
                    await connsEntry.SaveAsync(SafeWrite, metadata: TtlMeta, cancellationToken: ct);
                    currentCount = conns.Count;
                }

                return 0;
            }, ct);

            if (lastConnection)
            {
                await WithRetry(async () =>
                {
                    var metaEntry = await _dapr.GetStateEntryAsync<UserMeta>(Store, metaKey, cancellationToken: ct);
                    if (metaEntry.ETag is not null || metaEntry.Value is not null)
                    {
                        await metaEntry.DeleteAsync(SafeWrite, cancellationToken: ct);
                    }

                    return 0;
                }, ct);

                await WithRetry(async () =>
                {
                    var allEntry = await _dapr.GetStateEntryAsync<HashSet<string>>(Store, allKey, cancellationToken: ct);
                    var all = allEntry.Value ?? new HashSet<string>(StringComparer.Ordinal);
                    all.Remove(userId);
                    allEntry.Value = all;
                    await allEntry.SaveAsync(WeakWrite, metadata: TtlMeta, cancellationToken: ct);
                    return 0;
                }, ct);
            }

            return (lastConnection, currentCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing connection");
            throw;
        }
    }

    public async Task<IReadOnlyList<OnlineUserDto>> GetOnlineAsync(CancellationToken ct = default)
    {
        try
        {
            var all = await _dapr.GetStateAsync<HashSet<string>>(Store, PresenceKeys.All, cancellationToken: ct)
                      ?? new HashSet<string>(StringComparer.Ordinal);

            if (all.Count == 0)
            {
                return Array.Empty<OnlineUserDto>();
            }

            var metaKeys = all.Select(PresenceKeys.Meta).ToArray();
            var connsKeys = all.Select(PresenceKeys.Conns).ToArray();

            var metas = await _dapr.GetBulkStateAsync(Store, metaKeys, parallelism: 16, cancellationToken: ct);
            var conns = await _dapr.GetBulkStateAsync(Store, connsKeys, parallelism: 16, cancellationToken: ct);

            var list = new List<OnlineUserDto>(all.Count);
            var ids = all.ToArray();

            for (var i = 0; i < ids.Length; i++)
            {
                var userId = ids[i];
                var metaRaw = metas[i].Value;
                var connsRaw = conns[i].Value;

                if (string.IsNullOrWhiteSpace(metaRaw) || string.IsNullOrWhiteSpace(connsRaw))
                {
                    continue;
                }

                var metaObj = JsonSerializer.Deserialize<UserMeta>(metaRaw, _json);
                var connsSet = JsonSerializer.Deserialize<HashSet<string>>(connsRaw, _json) ?? new();

                if (metaObj is null)
                {
                    continue;
                }

                list.Add(new OnlineUserDto(userId, metaObj.Name, metaObj.Role, connsSet.Count));
            }

            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get online users");
            throw;
        }
    }
}
