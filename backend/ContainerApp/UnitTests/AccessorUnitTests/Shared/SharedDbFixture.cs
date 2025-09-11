using Accessor.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AccessorUnitTests.Shared;

public sealed class SharedDbFixture : IDisposable
{
    private readonly DbContextOptions<AccessorDbContext> _options;
    public AccessorDbContext Db { get; }

    public SharedDbFixture()
    {
        var dbName = "AccessorDb_Shared_For_UserRelations_Tests";

        _options = new DbContextOptionsBuilder<AccessorDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .EnableSensitiveDataLogging()
            .Options;

        Db = new AccessorDbContext(_options);

        Db.Database.EnsureCreated();
    }

    public Task ResetAsync()
    {
        Db.Database.EnsureDeleted();
        Db.Database.EnsureCreated();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Db.Dispose();
    }
}
