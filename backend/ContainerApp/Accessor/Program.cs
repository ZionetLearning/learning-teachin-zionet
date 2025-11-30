using System.Text.Json;
using Accessor.Constants;
using Accessor.DB;
using Accessor.Endpoints;
using Accessor.Models;
using Accessor.Models.QueueMessages;
using Accessor.Options;
using Accessor.Services;
using Accessor.Services.Avatars;
using Accessor.Services.Avatars.Models;
using Accessor.Services.Interfaces;
using Azure.Messaging.ServiceBus;
using DotQueue;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<DatabaseInitializer>();
builder.Services.AddScoped<IManagerCallbackQueueService, ManagerCallbackQueueService>();
builder.Services.AddScoped<IRefreshSessionService, RefreshSessionService>();
builder.Services.AddScoped<ISpeechService, SpeechService>();
builder.Services.AddScoped<IStudentPracticeHistoryService, StudentPracticeHistoryService>();
builder.Services.AddScoped<IWordCardService, WordCardService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<IAzureCommunicationService, AzureCommunicationService>();
builder.Services.AddScoped<IUserGameConfigurationService, UserGameConfigurationService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();

builder.Services.AddHttpClient("SpeechClient", client =>
{
    var region = builder.Configuration["Speech:Region"];
    var key = builder.Configuration["Speech:Key"];

    client.BaseAddress = new Uri($"https://{region}.api.cognitive.microsoft.com/");
    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
});

builder.Services.AddHttpClient<ILangfuseService, LangfuseService>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<LangfuseOptions>>().Value;
    // Only set BaseAddress if configured, otherwise use a dummy URL to prevent exception
    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        client.BaseAddress = new Uri(options.BaseUrl);
    }
    else
    {
        client.BaseAddress = new Uri("http://localhost");
    }
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

builder.Services.AddOptions<LangfuseOptions>()
    .Bind(builder.Configuration.GetSection("Langfuse"))
    .ValidateOnStart();

builder.Services.AddOptions<AvatarsOptions>()
    .Bind(builder.Configuration.GetSection(AvatarsOptions.SectionName));

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

// Configure PostgreSQL
builder.Services.AddSingleton(sp =>
{
    var connString = builder.Configuration.GetConnectionString("Postgres");
    var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(connString);
    dataSourceBuilder.EnableDynamicJson();
    return dataSourceBuilder.Build();
});

builder.Services.AddDbContextPool<AccessorDbContext>((sp, options) =>
{
    var dataSource = sp.GetRequiredService<Npgsql.NpgsqlDataSource>();
    options.UseNpgsql(dataSource, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    });
});

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

builder.Services.AddSingleton<IAvatarStorageService, AzureBlobAvatarStorageService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();
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
app.MapGamesEndpoints();
app.MapWordCardsEndpoints();
app.MapClassesEndpoints();
app.MapMeetingsEndpoints();
app.MapUserGameConfigurationEndpoints();
app.MapAchievementsEndpoints();

await app.RunAsync();
