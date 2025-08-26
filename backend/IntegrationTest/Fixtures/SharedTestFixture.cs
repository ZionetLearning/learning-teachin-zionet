
namespace IntegrationTests.Fixtures;

public class SharedTestFixture : IAsyncLifetime
{
    public HttpTestFixture HttpFixture { get; } = new();
    public TestUserFixture UserFixture { get; }

    public SharedTestFixture()
    {
        UserFixture = new TestUserFixture(HttpFixture);
    }

    public async Task InitializeAsync()
    {
        await UserFixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await UserFixture.DisposeAsync();
        HttpFixture.Dispose();
    }
}
