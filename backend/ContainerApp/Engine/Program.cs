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

builder.Services.AddSingleton(sp =>
{
    //var cfg = sp.GetRequiredService<IOptions<AzureOpenAiSettings>>().Value;

    //var kernel = Kernel.CreateBuilder()
    //             .AddAzureOpenAIChatCompletion(
    //                 deploymentName: cfg.DeploymentName,
    //                 endpoint: cfg.Endpoint,
    //                 apiKey: cfg.ApiKey)
    //             .Build();

    try
    {
        var config = builder.Configuration.GetSection("Claude");
        //var apiKey = config["ApiKey"];
        var model = config["ModelId"];
        var bedrockRuntime = new AmazonBedrockRuntimeClient(); // AWS creds are loaded from .env

#pragma warning disable SKEXP0070
        var kb = Kernel.CreateBuilder()
            .AddBedrockChatCompletionService(
                modelId: "anthropic.claude-3-haiku-20240307-v1:0",
                bedrockRuntime: bedrockRuntime,
                serviceId: "claude");

        //var logger = sp.GetRequiredService<ILoggerFactory>()
        //.CreateLogger("KernelPluginRegistration");
        //foreach (var plugin in sp.GetServices<ISemanticKernelPlugin>())
        //{
        //    try
        //    {
        //        var pluginName = plugin.GetType().ToPluginName();
        //        kernel.Plugins.AddFromObject(plugin, pluginName);
        //        logger.LogInformation("Plugin {Name} registered.", pluginName);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex,
        //            "Failed to register plugin {PluginType}", plugin.GetType().FullName);

        //    }
        //}

        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("KernelPluginRegistration");

        var baseDir = Path.Combine(AppContext.BaseDirectory, "Plugins", "Sentences");

        if (!Directory.Exists(baseDir))
        {
            logger.LogWarning("Plugins directory not found: {Dir}", baseDir);
        }
        else
        {
            try
            {
                kb.Plugins.AddFromPromptDirectory(baseDir, "Sentences");
                logger.LogInformation("Loaded SK plugin 'Sentences' from {Dir}", baseDir);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load SK plugin from {Dir}", baseDir);
            }
        }

        var kernel = kb.Build();
        return kernel;
    }
    catch (Exception)
    {
        var logger = sp.GetRequiredService<ILoggerFactory>();
        logger.CreateLogger("KernelPluginRegistration").LogInformation("faill!!!!!!!!!!!!!!!!!!!!!!!!!!.");
        throw;
    }
});

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
