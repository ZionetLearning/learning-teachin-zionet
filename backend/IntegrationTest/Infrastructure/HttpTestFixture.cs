using Microsoft.Extensions.Configuration;

namespace IntegrationTests.Infrastructure;

public class HttpTestFixture : IDisposable
{
    public HttpClient Client { get; }

    public HttpTestFixture()
    {
        var cfg = BuildConfig();

        var baseUrl = cfg.GetSection("TestSettings")["ApiBaseUrl"]
            ?? throw new InvalidOperationException(
                "TestSettings:ApiBaseUrl is missing. Add it to appsettings.json or appsettings.Local.json.");

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
            .AddEnvironmentVariables() // optional: lets CI override
            .Build();

    public void Dispose()
    {
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
