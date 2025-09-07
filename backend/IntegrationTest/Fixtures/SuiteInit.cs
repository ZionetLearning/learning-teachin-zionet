using Xunit.Abstractions;

namespace IntegrationTests.Fixtures
{
    public static class SuiteInit
    {
        private const string Key = "Auth+SignalR:Once";

        public static Task EnsureAsync(SharedTestFixture shared, SignalRTestFixture signalR, ITestOutputHelper? output)
            => AsyncOnce.EnsureAsync(Key, async () =>
            {
                await shared.GetAuthenticatedTokenAsync();
                await shared.EnsureSignalRStartedAsync(signalR, output);
            });
    }

}
