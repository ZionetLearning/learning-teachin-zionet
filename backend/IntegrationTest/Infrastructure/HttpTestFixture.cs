using DotNetEnv;

namespace IntegrationTests.Infrastructure;

public class HttpTestFixture : IDisposable
{
    public HttpClient Client { get; }

    public HttpTestFixture()
    {
        // Load variables from .env file
        Env.Load();

        var baseUrl = GetBaseUrl();

        Client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(40)
        };
    }

    private static string GetBaseUrl()
    {
        var url = Environment.GetEnvironmentVariable("API_BASE_URL");

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException(
                "API_BASE_URL is not set. Please define it in the .env file or environment variables."
            );
        }

        return url;
    }

    public void Dispose()
    {
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
