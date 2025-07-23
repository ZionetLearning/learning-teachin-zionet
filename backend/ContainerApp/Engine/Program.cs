using Engine.Endpoints;
using Engine.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDaprClient();
builder.Services.AddControllers().AddDapr();

builder.Services.AddScoped<IEngineService, EngineService>();

var app = builder.Build();

app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();
app.MapEngineEndpoints();

app.Run();
