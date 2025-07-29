using Accessor.Endpoints;
using Accessor.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers().AddDapr();

builder.Services.AddScoped<IAccessorService, AccessorService>();

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

app.Run();
