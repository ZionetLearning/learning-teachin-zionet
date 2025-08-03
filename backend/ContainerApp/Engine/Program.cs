using Engine.Endpoints;
using Engine.Models;
using Engine.Services;
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

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// For Log Scopes
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;             // this shows your BeginScope
    options.SingleLine = true;               //this gives you indentation and structure
    options.TimestampFormat = "[HH:mm:ss] ";  // optional, for nice timestamps
});


var app = builder.Build();

app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();
app.MapEngineEndpoints();
app.MapAiEndpoints();

app.Run();
