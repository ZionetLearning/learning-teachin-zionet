using DotNetEnv;

namespace IntegrationTests.Infrastructure;

public class AccessorHttpTestFixture : IDisposable
{
    public HttpClient Client { get; }

    public AccessorHttpTestFixture()
    {
        // Reuse your .env loader
        Env.Load();

        var baseUrl = GetAccessorBaseUrl();

        Client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(40)
        };
    }

    private static string GetAccessorBaseUrl()
    {
        // Prefer a Accessor-specific env var so the other tests can keep API_BASE_URL=5280
        var url = Environment.GetEnvironmentVariable("ACCESSOR_API_BASE_URL");
        if (string.IsNullOrWhiteSpace(url))
        {
            // Default to 5003 if not provided
            url = "http://localhost:5003";
        }
        return url;
    }

    public void Dispose()
    {
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
