using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

var builder = FunctionsApplication.CreateBuilder(args);

// 1. HTTP trigger + DI plumbing
builder.ConfigureFunctionsWebApplication();

// 2. (Optional) App Insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// 3. Service Bus client (only if you’ve set a real connection string)
var sbConn = Environment.GetEnvironmentVariable("ServiceBusConnection");
if (!string.IsNullOrWhiteSpace(sbConn))
{
    builder.Services.AddSingleton(sp => new ServiceBusClient(sbConn));
}

// 4. PostgreSQL factory
var pgConnStr = Environment.GetEnvironmentVariable("PostgreSqlConnection")
                ?? throw new InvalidOperationException("PostgreSqlConnection is not set");
builder.Services.AddSingleton<Func<NpgsqlConnection>>(sp => () =>
{
    // Each call to this Func returns a fresh NpgsqlConnection
    return new NpgsqlConnection(pgConnStr);
});
// in Program.cs (FunctionsStartup) or equivalent
builder.Services.AddHttpClient("accessor", client => {
    // If running in Azure, point to your Function App’s base URL
    client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("FUNCTIONS_BASE_URL")
                                 ?? "http://localhost:7071/");
});

// If you ever need to inject a raw NpgsqlConnection directly (not a Func), you can also do:
// builder.Services.AddTransient(sp => new NpgsqlConnection(pgConnStr));

builder.Build().Run();
