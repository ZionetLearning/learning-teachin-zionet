using Manager.Services;
using Manager.Services.Clients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Linq;

namespace Manager.UnitTests.TestHelpers;

public class CustomWebAppFactory : WebApplicationFactory<Program>
{
    public MocksBag Mocks { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            Remove<IAiGatewayService>(services);
            Remove<IManagerService>(services);
            Remove<IEngineClient>(services);
            Remove<IAccessorClient>(services);

            services.AddSingleton(Mocks.AiGatewayService.Object);
            services.AddSingleton(Mocks.ManagerService.Object);
            services.AddSingleton(Mocks.EngineClient.Object);
            services.AddSingleton(Mocks.AccessorClient.Object);
        });
    }

    static void Remove<T>(IServiceCollection services)
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (var d in descriptors) services.Remove(d);
    }

    public class MocksBag
    {
        public Moq.Mock<IAiGatewayService> AiGatewayService { get; } = new();
        public Moq.Mock<IManagerService> ManagerService { get; } = new();
        public Moq.Mock<IEngineClient> EngineClient { get; } = new();
        public Moq.Mock<IAccessorClient> AccessorClient { get; } = new();
    }
}
