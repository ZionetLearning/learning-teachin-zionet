using Azure.Messaging.ServiceBus;
using Engine.Constants;
using Engine.Endpoints;
using Engine.Messaging;
using Engine.Models;
using Engine.Services;
using Microsoft.Azure.Amqp.Sasl;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddDaprClient();
builder.Services.AddControllers().AddDapr();

builder.Services.AddScoped<IEngineService, EngineService>();
builder.Services.AddScoped<IChatAiService, ChatAiService>();
builder.Services.AddScoped<IAiReplyPublisher, AiReplyPublisher>();
builder.Services.AddMemoryCache();

builder.Services
    .AddOptions<AzureOpenAiSettings>()
    .Bind(builder.Configuration.GetSection("AzureOpenAI"))
    .ValidateDataAnnotations()
    .Validate(s =>
        !string.IsNullOrWhiteSpace(s.ApiKey) &&
        !string.IsNullOrWhiteSpace(s.Endpoint) &&
        !string.IsNullOrWhiteSpace(s.DeploymentName),
        "Azure OpenAI settings are incomplete");

builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<AzureOpenAiSettings>>().Value;

    return Kernel.CreateBuilder()
                 .AddAzureOpenAIChatCompletion(
                     deploymentName: cfg.DeploymentName,
                     endpoint: cfg.Endpoint,
                     apiKey: cfg.ApiKey)
                 .Build();
});

builder.Services.AddSingleton(_ =>
    new ServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]));
builder.Services.AddQueue<TaskModel, EngineQueueHandler>(
    QueueNames.ManagerToEngine,
    settings =>
    {
        settings.MaxConcurrentCalls = 5;
        settings.PrefetchCount = 10;
        settings.ProcessingDelayMs = 200;
        settings.MaxRetryAttempts = 3;
        settings.RetryDelaySeconds = 2;
    });
builder.Services.AddSingleton<ISystemPromptProvider, SystemPromptProvider>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();
app.MapAiEndpoints();

app.Run();
