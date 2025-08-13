using System;
using System.Net.Http;

namespace IntegrationTests.Infrastructure;

public sealed class AccessorHttpTestFixture : IDisposable
{
    public AccessorWebAppFactory Factory { get; } = new();
    public HttpClient Client { get; }

    public AccessorHttpTestFixture()
    {
        Client = Factory.CreateClient(); // in-process TestServer client (no Docker)
        Client.Timeout = TimeSpan.FromSeconds(40);
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
    }
}
