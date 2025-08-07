using Accessor.DB;
using Accessor.Endpoints;
using Accessor.Messaging;
using Accessor.Models;
using Accessor.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(sp =>
  new ServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]));

builder.Services.AddQueue<AccessorPayload, AccessorQueueHandler>(
    "accessor-queue",
    settings =>
    {
        settings.MaxConcurrentCalls = 4;
        settings.PrefetchCount = 8;
        settings.ProcessingDelayMs = 0;
        settings.MaxRetryAttempts = 3;
        settings.RetryDelaySeconds = 5;
    });

//builder.Services.AddQueue<UpdateTaskName, AccessorUpdateTaskNameHandler>(
//    QueueNames.TaskUpdateInput);

//builder.Services.AddSingleton<IQueueHandler<TaskModel>, AccessorCreateTaskHandler>();
//builder.Services.AddSingleton<IQueueHandler<UpdateTaskName>, AccessorUpdateTaskNameHandler>();

var env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddEndpointsApiExplorer();

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
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"), npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    }));
// This is required for the Scalar UI to have an option to setup an authentication token
builder.Services.AddOpenApi(
    "v1",
    options =>
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    }
);

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
if (env.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Accessor API";
        options.Theme = ScalarTheme.DeepSpace;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
        options.ShowSidebar = true;
        options.PersistentAuthentication = true;
        // here we can setup a default token
        //options.AddPreferredSecuritySchemes("Bearer")
        // .AddHttpAuthentication("Bearer", auth =>
        // {
        //     auth.Token = "Some Auth Token...";
        // });

    });
}
// Map endpoints (routes)
app.MapAccessorEndpoints();
await app.RunAsync();
