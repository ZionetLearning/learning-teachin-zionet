namespace IntegrationTests.Fixtures;

[CollectionDefinition("Shared test collection")]
public class SharedTestCollection
    : ICollectionFixture<SharedTestFixture>,
      ICollectionFixture<SignalRTestFixture>
{ }

[CollectionDefinition("Minimal test collection")]
public class MinimalTestCollection
    : ICollectionFixture<MinimalSharedTestFixture>,
      ICollectionFixture<SignalRTestFixture>
{ }
