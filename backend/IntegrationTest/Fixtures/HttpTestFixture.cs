using IntegrationTests.Constants;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace IntegrationTests.Fixtures;

public class HttpTestFixture : IDisposable
{
    public HttpClient Client { get; }
    private readonly HttpTestFixture _http;

    public HttpTestFixture()
    {
        var cfg = BuildConfig();

        var baseUrl = cfg.GetSection("TestSettings")["ApiBaseUrl"]
            ?? throw new InvalidOperationException(
                "TestSettings:ApiBaseUrl is missing. Add it to appsettings.json or appsettings.Local.json.");

        var handler = new HttpClientHandler();

        Client = new HttpClient(handler)
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(40)
        };

        Client.DefaultRequestHeaders.Add("X-User-Id", TestConstants.TestUserId);
    }

    private static IConfigurationRoot BuildConfig() =>
        new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables() // optional: lets CI override
            .Build();
    public async Task<string> AuthenticateAndGetTokenAsync(string email, string password)
    {
        var response = await _http.Client.PostAsJsonAsync("/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();

        var loginResult = await response.Content.ReadFromJsonAsync<LoginResponse>();
        if (loginResult?.AccessToken is null)
            throw new InvalidOperationException("No access token returned from /auth/login");

        return loginResult.AccessToken;
    }

    public void Dispose()
    {
        Client?.Dispose();
        GC.SuppressFinalize(this);
    }
}
public record LoginResponse(string AccessToken, string RefreshToken);
