namespace IntegrationTests.Fixtures;

[CollectionDefinition("IntegrationTests")]
public class IntegrationTestsCollection : ICollectionFixture<HttpClientFixture>,
      ICollectionFixture<SignalRTestFixture>
{ }
