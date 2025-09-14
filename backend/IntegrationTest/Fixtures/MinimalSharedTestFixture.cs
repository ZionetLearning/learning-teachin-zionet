namespace IntegrationTests.Fixtures;

public class MinimalSharedTestFixture : IAsyncLifetime
{
    public HttpTestFixture HttpFixture { get; } = new();

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync()
    {
        HttpFixture.Dispose();
        return Task.CompletedTask;
    }
}
