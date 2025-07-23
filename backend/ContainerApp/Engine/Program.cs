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

    var endpoint = cfg["AzureOpenAI:Endpoint"] ?? throw new("Endpoint?");
    var apiKey = cfg["AzureOpenAI:ApiKey"] ?? throw new("ApiKey?");
    var deployment = cfg["AzureOpenAI:DeploymentName"] ?? throw new("Deployment?");

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
