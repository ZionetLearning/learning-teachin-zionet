using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Xunit;

namespace IntegrationTests.Infrastructure
{
    public class HttpTestFixture : IDisposable
    {
        public HttpClient Client { get; }

        public HttpTestFixture()
        {
            Client = new HttpClient
            {
                BaseAddress = new Uri(GetBaseUrl()),
                Timeout = TimeSpan.FromSeconds(40)
            };
        }

        private static string GetBaseUrl()
        {
            return Environment.GetEnvironmentVariable("API_BASE_URL")
                ?? "http://localhost:5001";
        }

        public void Dispose()
        {
            Client?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}