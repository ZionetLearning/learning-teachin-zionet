namespace IntegrationTests.Fixtures;

[CollectionDefinition("IntegrationTests")]
public class IntegrationTestsCollection : ICollectionFixture<HttpClientFixture>,
      ICollectionFixture<SignalRTestFixture>
{ }

[CollectionDefinition("Per-test user collection")]
public class PerTestUserCollection
    : ICollectionFixture<PerTestUserFixture>
{ }
