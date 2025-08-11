using System.Net.Http;
using Dapr.Client;

namespace Manager.UnitTests.TestHelpers;

public interface IDaprClientWrapper
{
    Task<T?> InvokeMethodAsync<T>(HttpMethod method, string appId, string methodName, CancellationToken ct = default);
    Task InvokeMethodAsync(HttpMethod method, string appId, string methodName, CancellationToken ct = default);
    Task InvokeBindingAsync(string bindingName, string operation, object data, CancellationToken ct = default);
}
