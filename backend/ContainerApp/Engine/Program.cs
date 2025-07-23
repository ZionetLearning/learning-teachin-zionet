using Engine.Endpoints;
using Engine.Services;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDaprClient();
builder.Services.AddControllers().AddDapr();

builder.Services.AddScoped<IEngineService, EngineService>();

builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();

    var section = cfg.GetSection("AzureOpenAI");
    var endpoint = section["Endpoint"];
    var apiKey = section["ApiKey"];
    var deployment = section["DeploymentName"];

    if (string.IsNullOrWhiteSpace(endpoint))
        throw new InvalidOperationException("AzureOpenAI:Endpoint not set in config or environment variables");

    if (string.IsNullOrWhiteSpace(apiKey))
        throw new InvalidOperationException("AzureOpenAI:ApiKey not set - check appsettings.json and environment variables");

    if (string.IsNullOrWhiteSpace(deployment))
        throw new InvalidOperationException("AzureOpenAI:DeploymentName not set");

    return Kernel.CreateBuilder()
                 .AddAzureOpenAIChatCompletion(
                     deploymentName: deployment,
                     endpoint: endpoint,
                     apiKey: apiKey)
                 .Build();
});


builder.Services.AddScoped<IChatAiService, ChatAiService>();
builder.Services.AddScoped<IAiReplyPublisher, AiReplyPublisher>();

var app = builder.Build();

app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();
app.MapEngineEndpoints();
app.MapAiEndpoints();

app.Run();
