using System.Text.Json;
using Dapr.Client;
using Manager.Constants;
using Manager.Models.Users;

namespace Manager.Services;
public interface IOnlinePresenceService
{
    Task<bool> AddConnectionAsync(string userId, string name, string role, string connectionId, CancellationToken ct = default);
    Task<bool> RemoveConnectionAsync(string userId, string connectionId, CancellationToken ct = default);
    Task<IReadOnlyList<OnlineUserDto>> GetOnlineAsync(CancellationToken ct = default);
}

public sealed class OnlinePresenceService : IOnlinePresenceService
{
    private readonly DaprClient _dapr;
    private const string Store = AppIds.StateStore;
    public OnlinePresenceService(DaprClient dapr) => _dapr = dapr;

    public async Task<bool> AddConnectionAsync(string userId, string name, string role, string connectionId, CancellationToken ct = default)
    {
        var connsKey = PresenceKeys.Conns(userId);
        var metaKey = PresenceKeys.Meta(userId);
        var allKey = PresenceKeys.All;

        var connsEntry = await _dapr.GetStateEntryAsync<HashSet<string>>(Store, connsKey, cancellationToken: ct);
        var allEntry = await _dapr.GetStateEntryAsync<HashSet<string>>(Store, allKey, cancellationToken: ct);

        var conns = connsEntry.Value ?? new HashSet<string>(StringComparer.Ordinal);
        var all = allEntry.Value ?? new HashSet<string>(StringComparer.Ordinal);

        var wasEmpty = conns.Count == 0;
        conns.Add(connectionId);

        var ops = new List<StateTransactionRequest>
        {
            new(connsKey, JsonSerializer.SerializeToUtf8Bytes(conns), StateOperationType.Upsert)
        };

        var firstConnection = false;

        if (wasEmpty && conns.Count == 1)
        {
            firstConnection = true;
            var meta = new UserMeta(name, role);
            all.Add(userId);

            ops.Add(new(metaKey, JsonSerializer.SerializeToUtf8Bytes(meta), StateOperationType.Upsert));
            ops.Add(new(allKey, JsonSerializer.SerializeToUtf8Bytes(all), StateOperationType.Upsert));
        }

        await _dapr.ExecuteStateTransactionAsync(Store, ops, cancellationToken: ct);
        return firstConnection;
    }

    public async Task<bool> RemoveConnectionAsync(string userId, string connectionId, CancellationToken ct = default)
    {
        var connsKey = PresenceKeys.Conns(userId);
        var metaKey = PresenceKeys.Meta(userId);
        var allKey = PresenceKeys.All;

        var connsEntry = await _dapr.GetStateEntryAsync<HashSet<string>>(Store, connsKey, cancellationToken: ct);
        var allEntry = await _dapr.GetStateEntryAsync<HashSet<string>>(Store, allKey, cancellationToken: ct);

        var conns = connsEntry.Value ?? new HashSet<string>(StringComparer.Ordinal);
        var all = allEntry.Value ?? new HashSet<string>(StringComparer.Ordinal);

        conns.Remove(connectionId);

        var ops = new List<StateTransactionRequest>();

        var lastConnection = false;

        if (conns.Count == 0)
        {
            lastConnection = true;
            all.Remove(userId);

            ops.Add(new(connsKey, null, StateOperationType.Delete));
            ops.Add(new(metaKey, null, StateOperationType.Delete));
            ops.Add(new(allKey, JsonSerializer.SerializeToUtf8Bytes(all), StateOperationType.Upsert));
        }
        else
        {
            ops.Add(new(connsKey, JsonSerializer.SerializeToUtf8Bytes(conns), StateOperationType.Upsert));
        }

        await _dapr.ExecuteStateTransactionAsync(Store, ops, cancellationToken: ct);
        return lastConnection;
    }

    public async Task<IReadOnlyList<OnlineUserDto>> GetOnlineAsync(CancellationToken ct = default)
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

            var metaObj = JsonSerializer.Deserialize<UserMeta>(metaRaw);
            var connsSet = JsonSerializer.Deserialize<HashSet<string>>(connsRaw) ?? new();

            if (metaObj is null)
            {
                continue;
            }

            list.Add(new OnlineUserDto(
                userId,
                metaObj.Name,
                metaObj.Role,
                connsSet.Count
            ));
        }

        return list;
    }
}
