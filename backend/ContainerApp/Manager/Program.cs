using Manager.Endpoints;
using Manager.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDaprClient();
builder.Services.AddControllers().AddDapr();

builder.Services.AddScoped<IManagerService, ManagerService>();
builder.Services.AddScoped<IAiGatewayService, AiGatewayService>();

var app = builder.Build();

app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();
app.MapManagerEndpoints();
app.MapAiEndpoints();

app.Run();
