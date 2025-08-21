using AuthComponentTests;
using Xunit;

[CollectionDefinition("Auth Test Collection")]
public class AuthTestCollection : ICollectionFixture<AuthTestFixture>
{
    // This class has no code, and is never created. It's just a [CollectionDefinition].
}
