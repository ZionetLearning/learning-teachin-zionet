using IntegrationTests.Infrastructure;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Auth;

public abstract class AuthTestBase : IntegrationTestBase
{
    protected AuthTestBase(HttpTestFixture fixture, ITestOutputHelper outputHelper, SignalRTestFixture signalRFixture)
        : base(fixture, outputHelper, signalRFixture)
    {
    }

    // Optional: You can add shared auth logic here later
}
