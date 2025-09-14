using System.Text.Json;
using Accessor.Constants;
using Accessor.DB;
using Accessor.Endpoints;
using Accessor.Models;
using Accessor.Models.QueueMessages;
using Accessor.Options;
using Accessor.Services;
using Accessor.Services.Interfaces;
using Azure.Messaging.ServiceBus;
using DotQueue;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Timeouts; // <-- Added for RequestTimeouts
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services & configuration ----------

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

// ---------- Dapr client: JSON + Global timeout ----------
builder.Services.AddDaprClient((serviceProvider, daprBuilder) =>
{
    // Keep your JSON settings
    daprBuilder.UseJsonSerializationOptions(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new UtcDateTimeOffsetConverter() }
    });

    // One place to control timeout across all Dapr calls
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var timeoutSeconds = config.GetValue<int?>("Timeouts:DaprClientSeconds") ?? 30;
    daprBuilder.UseTimeout(TimeSpan.FromSeconds(timeoutSeconds)); // global Dapr call timeout
});

// ---------- PostgreSQL ----------
builder.Services.AddDbContext<AccessorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"), npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    }));

// ---------- OpenAPI / Scalar ----------
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

// ---------- Request timeouts (GLOBAL) ----------
var requestTtlSeconds = builder.Configuration.GetValue<int?>("Timeouts:RequestSeconds") ?? 30;
builder.Services.AddRequestTimeouts(options =>
{
    options.DefaultPolicy = new RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromSeconds(requestTtlSeconds),
        TimeoutStatusCode = StatusCodes.Status408RequestTimeout
        // You can add WriteTimeoutResponse here if you want custom body
    };
});

// ---------- Build ----------
var app = builder.Build();

// ---------- DB + prompt init ----------
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();

    var promptStartup = scope.ServiceProvider.GetRequiredService<IPromptService>();
    await promptStartup.InitializeDefaultPromptsAsync();
}

// ---------- Middleware pipeline ----------
app.UseCloudEvents();
app.MapSubscribeHandler();

// Enable request timeouts BEFORE mapping endpoints
app.UseRequestTimeouts();

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
        // Example to pre-configure auth if needed:
        // options.AddPreferredSecuritySchemes("Bearer");
    });
}

// ---------- Endpoints ----------
app.MapTasksEndpoints();
app.MapChatsEndpoints();
app.MapPromptEndpoints();
app.MapUsersEndpoints();
app.MapAuthEndpoints();
app.MapRefreshSessionEndpoints();
app.MapStatsEndpoints();
app.MapMediaEndpoints();

// Simple health check endpoint for Kubernetes probes
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }))
    .WithName("HealthCheck")
    .WithTags("Health")
    .Produces(StatusCodes.Status200OK);

await app.RunAsync();
