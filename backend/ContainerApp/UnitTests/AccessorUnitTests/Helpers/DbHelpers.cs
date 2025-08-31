using Accessor.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AccessorUnitTests.Helpers;

public static class DbHelpers
{
    public static AccessorDbContext NewInMemoryDb(string? name = null)
    {
        var dbName = name ?? Guid.NewGuid().ToString("N");
        var opts = new DbContextOptionsBuilder<AccessorDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .EnableSensitiveDataLogging()
            .Options;

        return new AccessorDbContext(opts);
    }
}
