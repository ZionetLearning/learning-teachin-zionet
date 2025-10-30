using Amazon.BedrockRuntime;
using Azure.Messaging.ServiceBus;
using dotenv.net;
using DotQueue;
using Engine;
using Engine.Constants;
using Engine.Constants.Chat;
using Engine.Endpoints;
using Engine.Models;
using Engine.Models.QueueMessages;
using Engine.Options;
using Engine.Plugins;
using Engine.Services;
using Engine.Services.Clients.AccessorClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<PromptKeyOptions>(builder.Configuration.GetSection("Prompts:Keys"));
var promptKeyOptions = builder.Configuration.GetSection("Prompts:Keys").Get<PromptKeyOptions>() ?? new();
PromptsKeys.Configure(promptKeyOptions);

builder.Services.AddDaprClient();
builder.Services.AddControllers().AddDapr();

builder.Services.AddScoped<IChatTitleService, ChatTitleService>();
builder.Services.AddScoped<IChatAiService, ChatAiService>();
builder.Services.AddScoped<IAiReplyPublisher, AiReplyPublisher>();
builder.Services.AddScoped<IAccessorClient, AccessorClient>();
builder.Services.AddScoped<ISentencesService, SentencesService>();
builder.Services.AddSingleton<IRetryPolicyProvider, RetryPolicyProvider>();
builder.Services.AddSingleton<IRetryPolicy, RetryPolicy>();
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddSingleton<ISemanticKernelPlugin, TimePlugin>();

builder.Services.AddMemoryCache();
builder.Services
       .AddOptions<MemoryCacheEntryOptions>()
       .Configure<IConfiguration>((opt, cfg) =>
       {
           var section = cfg.GetSection("CacheOptions");
       });

builder.Services
    .AddOptions<AzureOpenAiSettings>()
    .Bind(builder.Configuration.GetSection("AzureOpenAI"))
    .ValidateDataAnnotations()
    .Validate(s =>
        !string.IsNullOrWhiteSpace(s.ApiKey) &&
        !string.IsNullOrWhiteSpace(s.Endpoint) &&
        !string.IsNullOrWhiteSpace(s.DeploymentName),
        "Azure OpenAI settings are incomplete");

// Azure OpenAI Semantic Kernel with Plugins
builder.Services.AddSingleton(sp =>
{
    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("KernelPluginRegistration");

    try
    {
        var azureCfg = sp.GetRequiredService<IOptions<AzureOpenAiSettings>>().Value;
        var claudeCfg = builder.Configuration.GetSection("Claude");
        var phiCfg = builder.Configuration.GetSection("Phi-4");
        if (claudeCfg is null || phiCfg is null)
        {
            throw new InvalidOperationException("Claude or Phi-4 configuration section is missing.");
        }

        var kb = Kernel.CreateBuilder()
            // GPT-4.1-mini
            .AddAzureOpenAIChatCompletion(
                deploymentName: azureCfg.DeploymentName,
                endpoint: azureCfg.Endpoint,
                apiKey: azureCfg.ApiKey,
                serviceId: "gpt")
            // Claude-3-haiku
            .AddBedrockChatCompletionService(
                modelId: claudeCfg["ModelId"]!,
                bedrockRuntime: new AmazonBedrockRuntimeClient(),
                serviceId: "claude")
            // Phi-4
            .AddAzureOpenAIChatCompletion(
            deploymentName: phiCfg["DeploymentName"]!,
            endpoint: phiCfg["Endpoint"]!,
            apiKey: phiCfg["ApiKey"]!,
            serviceId: "phi");

        LoadPlugins(kb, logger);
        return kb.Build();
    }
    catch (Exception)
    {
        logger.LogWarning("Failed to create Semantic Kernel instance.");
        throw;
    }
});

static void LoadPlugins(IKernelBuilder kb, ILogger logger)
{
    var baseDir = Path.Combine(AppContext.BaseDirectory, "Plugins", "Sentences");

    if (!Directory.Exists(baseDir))
    {
        logger.LogWarning("Plugins directory not found: {Dir}", baseDir);
        return;
    }

    try
    {
        kb.Plugins.AddFromPromptDirectory(baseDir, "Sentences");
        logger.LogInformation("Loaded plugin 'Sentences' from {Dir}", baseDir);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to load plugin from {Dir}", baseDir);
    }
}

builder.Services.AddKeyedSingleton("gen", (sp, key) =>
{
    var cfg = sp.GetRequiredService<IOptions<AzureOpenAiSettings>>().Value;

    var kb = Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion(
            deploymentName: cfg.DeploymentName,
            endpoint: cfg.Endpoint,
            apiKey: cfg.ApiKey);

    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("KernelGenPluginRegistration");

    var dir = Path.Combine(AppContext.BaseDirectory, "Plugins", "Sentences");
    try
    {
        kb.Plugins.AddFromPromptDirectory(dir, "Sentences");
        logger.LogInformation("Prompt plugin 'Sentences' loaded from {Dir}", dir);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to load prompt plugin from {Dir}", dir);
    }

    var kernel = kb.Build();

    try
    {
        var info = string.Join("; ", kernel.Plugins.Select(p =>
            $"{p.Name}: [" + string.Join(", ", p.Select(f => f.Name)) + "]"));
        logger.LogInformation("Loaded SK plugins & functions: {Info}", info);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to enumerate SK plugins/functions");
    }

    return kernel;
});

builder.Services.AddSingleton(_ =>
    new ServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]));

builder.Services.AddQueue<Message, EngineQueueHandler>(
    QueueNames.EngineQueue,
    settings =>
    {
        settings.MaxConcurrentCalls = 5;
        settings.PrefetchCount = 10;
        settings.ProcessingDelayMs = 200;
        settings.MaxRetryAttempts = 3;
        settings.RetryDelaySeconds = 2;
    });

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();
app.MapAiEndpoints();

app.Run();
