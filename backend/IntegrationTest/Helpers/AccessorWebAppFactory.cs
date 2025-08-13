using Accessor.DB;                      // your DbContext
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Linq;

namespace IntegrationTests.Infrastructure;

public sealed class AccessorWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(static services =>
        {
            // Replace Postgres with EF InMemory for fast, isolated tests
            var dbDesc = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AccessorDbContext>));
            if (dbDesc != null)
                services.Remove(dbDesc);

            services.AddDbContext<AccessorDbContext>(o =>
                o.UseInMemoryDatabase($"accessor-tests-{Guid.NewGuid()}"));

            // Ensure DB exists
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AccessorDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
