using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Azure.Messaging.ServiceBus;
using Npgsql;

[assembly: FunctionsStartup(typeof(AzureFunctionsProject.Startup))]
namespace AzureFunctionsProject
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Service Bus client for all functions
            builder.Services.AddSingleton(sp =>
              new ServiceBusClient(Environment.GetEnvironmentVariable("ServiceBusConnection")));
            // PostgreSQL connection factory
            builder.Services.AddTransient(sp =>
              new NpgsqlConnection(Environment.GetEnvironmentVariable("PostgreSqlConnection")));
            // Application Insights
            builder.Services.AddApplicationInsightsTelemetryWorkerService();
        }
    }
}
