using Azure.Messaging.ServiceBus;
using AzureFunctionsProject.Manager;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using AzureFunctionsProject.Services;

var builder = FunctionsApplication.CreateBuilder(args);

// 1. HTTP trigger + DI plumbing
builder.ConfigureFunctionsWebApplication();

// 2. (Optional) Application Insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// 3. Service Bus client (fail fast if not configured)
var sbConn = builder.Configuration.GetValue<string>("ServiceBusConnection")
             ?? throw new InvalidOperationException(
                    "ServiceBusConnection must be defined in configuration (e.g. local.settings.json or Azure App Settings)");
builder.Services.AddSingleton(sp => new ServiceBusClient(sbConn));

// 4. PostgreSQL connection factory
var pgConnStr = builder.Configuration.GetValue<string>("PostgreSqlConnection")
                ?? throw new InvalidOperationException(
                       "PostgreSqlConnection must be defined in configuration");
builder.Services.AddSingleton<Func<NpgsqlConnection>>(sp =>
    () => new NpgsqlConnection(pgConnStr)
);

// 5. Accessor HTTP client
builder.Services
     .AddHttpClient<IAccessorClient, AccessorClient>(client =>
     {
    client.BaseAddress = new Uri(
    builder.Configuration
                        .GetValue<string>("FUNCTIONS_BASE_URL")
                 ?? "http://localhost:7071/");
         });
    // 6. Engine HTTP client (same base URL)
    builder.Services
         .AddHttpClient<IEngineClient, EngineClient>(client =>
         {
    client.BaseAddress = new Uri(
    builder.Configuration.GetValue<string>("FUNCTIONS_BASE_URL")
    ?? "http://localhost:7278/");});

builder.Services.AddSingleton<IDataService, DataService>();

builder.Build().Run();
