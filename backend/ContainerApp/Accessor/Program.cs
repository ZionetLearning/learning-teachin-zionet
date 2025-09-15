using System.Text.Json;
using Accessor.Constants;
using Accessor.DB;
using Accessor.Endpoints;
using Accessor.Models;
using Accessor.Models.QueueMessages;
using Accessor.Options;
using Accessor.Services;
using Accessor.Services.Interfaces;
using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using DotQueue;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(sp =>
  new ServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]));
builder.Services.AddSingleton<IRetryPolicyProvider, RetryPolicyProvider>();
builder.Services.AddSingleton<IRetryPolicy, RetryPolicy>();
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

builder.Services.AddQueue<Message, AccessorQueueHandler>(
    QueueNames.AccessorQueue,
    settings =>
    {
        settings.MaxConcurrentCalls = 4;
        settings.PrefetchCount = 8;
        settings.ProcessingDelayMs = 0;
        settings.MaxRetryAttempts = 3;
        settings.RetryDelaySeconds = 5;
    });

builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<DatabaseInitializer>();
builder.Services.AddScoped<IManagerCallbackQueueService, ManagerCallbackQueueService>();
builder.Services.AddScoped<IRefreshSessionService, RefreshSessionService>();
builder.Services.AddScoped<ISpeechService, SpeechService>();

builder.Services.AddHttpClient("SpeechClient", client =>
{
    var region = builder.Configuration["Speech:Region"];
    var key = builder.Configuration["Speech:Key"];

    client.BaseAddress = new Uri($"https://{region}.api.cognitive.microsoft.com/");
    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
});

builder.Services.AddScoped<IPromptService, PromptService>();

var env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddJsonFile("prompts.defaults.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOptions<TaskCacheOptions>()
    .Bind(builder.Configuration.GetSection("TaskCache"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<PromptsOptions>()
    .Bind(builder.Configuration.GetSection("Prompts"))
    .ValidateOnStart();

// Register Dapr client with custom JSON options
builder.Services.AddDaprClient(client =>
{
    client.UseJsonSerializationOptions(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new UtcDateTimeOffsetConverter() }
    });
});

// Configure PostgreSQL with Managed Identity support
string? postgresEndpoint = builder.Configuration["POSTGRES_ENDPOINT"];
string? postgresClientId = builder.Configuration["POSTGRES_CLIENT_ID"];

NpgsqlDataSource? dataSource = null;
if (!string.IsNullOrEmpty(postgresEndpoint) && !string.IsNullOrEmpty(postgresClientId))
{
    // Use Managed Identity for PostgreSQL authentication
    var connectionBuilder = new NpgsqlConnectionStringBuilder(postgresEndpoint);
    connectionBuilder.SslMode = SslMode.Require;
    connectionBuilder.Password = null; // Remove any password
    
    Console.WriteLine($"PostgreSQL: Using Managed Identity authentication");
    Console.WriteLine($"PostgreSQL: Endpoint: {postgresEndpoint}");
    Console.WriteLine($"PostgreSQL: Client ID: {postgresClientId}");
    
    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionBuilder.ToString());
    dataSourceBuilder.UsePeriodicPasswordProvider(async (_, ct) =>
    {
        try
        {
            // Try WorkloadIdentityCredential first (for AKS with Azure Workload Identity)
            var credential = new ChainedTokenCredential(
                new WorkloadIdentityCredential(new WorkloadIdentityCredentialOptions
                {
                    ClientId = postgresClientId
                }),
                new ManagedIdentityCredential(postgresClientId)
            );
            
            var accessToken = await credential.GetTokenAsync(
                new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]), ct);
            
            Console.WriteLine($"PostgreSQL: Successfully obtained access token, expires: {accessToken.ExpiresOn}");
            return accessToken.Token;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PostgreSQL: Failed to get access token: {ex.Message}");
            throw;
        }
    }, TimeSpan.FromMinutes(55), TimeSpan.FromSeconds(10));
    
    dataSource = dataSourceBuilder.Build();
}

if (dataSource != null)
{
    // Use Managed Identity data source
    builder.Services.AddDbContext<AccessorDbContext>(options =>
        options.UseNpgsql(dataSource, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
        }));
}
else
{
    // Fallback to connection string
    Console.WriteLine("PostgreSQL: Using connection string authentication");
    builder.Services.AddDbContext<AccessorDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"), npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null);
        }));
}

// This is required for the Scalar UI to have an option to setup an authentication token
builder.Services.AddOpenApi(
    "v1",
    options =>
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    }
);

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new UtcDateTimeOffsetConverter());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();
    var promptStartup = scope.ServiceProvider.GetRequiredService<IPromptService>();
    await promptStartup.InitializeDefaultPromptsAsync();
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
app.MapTasksEndpoints();
app.MapChatsEndpoints();
app.MapPromptEndpoints();
app.MapUsersEndpoints();
app.MapAuthEndpoints();
app.MapRefreshSessionEndpoints();
app.MapStatsEndpoints();
app.MapMediaEndpoints();
await app.RunAsync();
