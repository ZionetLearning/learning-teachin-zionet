using Accessor.DB;
using Accessor.Endpoints;
using Accessor.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers().AddDapr();

builder.Services.AddScoped<IAccessorService, AccessorService>();

// Add internal configuration to the application
builder.Configuration.AddInMemoryCollection(Accessor.InternalConfiguration.Default!);

// Register Dapr client with custom JSON options
builder.Services.AddDaprClient(client =>
{
    client.UseJsonSerializationOptions(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    });
});

// Configure PostgreSQL
builder.Services.AddDbContext<AccessorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var startupService = scope.ServiceProvider.GetRequiredService<IAccessorService>();
    await startupService.InitializeAsync();
}

// Configure middleware and Dapr
app.UseCloudEvents();
app.MapSubscribeHandler();

// Map endpoints (routes)
app.MapAccessorEndpoints();
await app.RunAsync();
