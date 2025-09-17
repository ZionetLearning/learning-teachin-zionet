namespace IntegrationTests.Fixtures;

[CollectionDefinition("Shared test collection")]
public class SharedTestCollection
    : ICollectionFixture<SharedTestFixture>,
      ICollectionFixture<SignalRTestFixture>
{ }

[CollectionDefinition("Per-test user collection")]
public class PerTestUserCollection
    : ICollectionFixture<PerTestUserFixture>
{ }
