using Microsoft.Extensions.Configuration;

namespace IntegrationTests.Infrastructure;

public class AccessorHttpTestFixture : IDisposable
{
    public HttpClient Client { get; }

    public AccessorHttpTestFixture()
    {
        var cfg = BuildConfig();

        // Reuse the same TestSettings unless you choose to split later
        var baseUrl = cfg.GetSection("TestSettings")["ApiBaseUrl"]
            ?? "http://localhost:5003"; // fallback if you run Accessor separately

        Client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(40)
        };
    }

    private static IConfigurationRoot BuildConfig() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

    public void Dispose()
    {
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }
}