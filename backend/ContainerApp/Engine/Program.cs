using Azure;
using Azure.AI.OpenAI;
using Azure.Messaging.ServiceBus;
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
using Engine.Tools;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddJsonFile("prompts.config.json", optional: false, reloadOnChange: true)
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
builder.Services.AddScoped<IWordExplainService, WordExplainService>();

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
    var cfg = sp.GetRequiredService<IOptions<AzureOpenAiSettings>>().Value;

    return new AzureOpenAIClient(
        new Uri(cfg.Endpoint),
        new AzureKeyCredential(cfg.ApiKey));
});

builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<AzureOpenAiSettings>>().Value;

    //delete after migration
    var kernel = Kernel.CreateBuilder()
                 .AddAzureOpenAIChatCompletion(
                     deploymentName: cfg.DeploymentName,
                     endpoint: cfg.Endpoint,
                     apiKey: cfg.ApiKey)
                 .Build();
    var logger = sp.GetRequiredService<ILoggerFactory>()
    .CreateLogger("KernelPluginRegistration");
    foreach (var plugin in sp.GetServices<ISemanticKernelPlugin>())
    {
        try
        {
            var pluginName = plugin.GetType().ToPluginName();
            kernel.Plugins.AddFromObject(plugin, pluginName);
            logger.LogInformation("Plugin {Name} registered.", pluginName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to register plugin {PluginType}", plugin.GetType().FullName);

        }
    }

    return kernel;

});

builder.Services.AddSingleton<TimeTools>();

builder.Services.AddSingleton<IList<AITool>>(sp =>
{
    var time = sp.GetRequiredService<TimeTools>();

    var getCurrentTimeTool = AIFunctionFactory.Create(
        time.GetCurrentTime,
        new AIFunctionFactoryOptions
        {
            Name = "get_current_time",
            Description = "Returns the current time in ISO-8601 (UTC)."
        });

    return new List<AITool> { getCurrentTimeTool };
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

    var sentencesDir = Path.Combine(AppContext.BaseDirectory, "Plugins", "Sentences");
    try
    {
        kb.Plugins.AddFromPromptDirectory(sentencesDir, "Sentences");
        logger.LogInformation("Prompt plugin 'Sentences' loaded from {Dir}", sentencesDir);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to load prompt plugin from {Dir}", sentencesDir);
    }

    var wordExplainDir = Path.Combine(AppContext.BaseDirectory, "Plugins", "WordExplain");
    try
    {
        kb.Plugins.AddFromPromptDirectory(wordExplainDir, "WordExplain");
        logger.LogInformation("Prompt plugin 'WordExplain' loaded from {Dir}", wordExplainDir);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to load prompt plugin from {Dir}", wordExplainDir);
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
