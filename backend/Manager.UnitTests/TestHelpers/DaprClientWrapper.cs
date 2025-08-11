using System.Net.Http;
using Dapr.Client;
using Manager.Services.Clients;

namespace Manager.UnitTests.TestHelpers;

public sealed class DaprClientWrapper : IDaprClientWrapper
{
    private readonly DaprClient _inner;
    public DaprClientWrapper(DaprClient inner) => _inner = inner;

    public Task<T?> InvokeMethodAsync<T>(HttpMethod method, string appId, string methodName, CancellationToken ct = default) =>
        _inner.InvokeMethodAsync<T>(method, appId, methodName, ct);

    public Task InvokeMethodAsync(HttpMethod method, string appId, string methodName, CancellationToken ct = default) =>
        _inner.InvokeMethodAsync(method, appId, methodName, ct);

    public Task InvokeBindingAsync(string bindingName, string operation, object data, CancellationToken ct = default) =>
        _inner.InvokeBindingAsync(bindingName, operation, data, cancellationToken: ct);
}
