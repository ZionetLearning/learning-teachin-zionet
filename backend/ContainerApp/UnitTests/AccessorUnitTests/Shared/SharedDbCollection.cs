using Xunit;

namespace AccessorUnitTests.Shared;

[CollectionDefinition("SharedDb", DisableParallelization = true)]
public class SharedDbCollection : ICollectionFixture<SharedDbFixture>
{
}
