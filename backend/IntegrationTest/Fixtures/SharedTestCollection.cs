// File: SharedTestCollection.cs
using Xunit;

namespace IntegrationTests.Fixtures;

[CollectionDefinition("Shared test collection")]
public class SharedTestCollection : ICollectionFixture<SharedTestFixture> { }
