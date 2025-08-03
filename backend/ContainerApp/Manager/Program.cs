using Manager.Endpoints;
using Manager.Hubs;
using Manager.Services;
using Manager.Services.Clients;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// ---- Services ----
builder.Services.AddControllers();

builder.Services.AddControllers().AddDapr();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddScoped<IManagerService, ManagerService>();
builder.Services.AddScoped<IAiGatewayService, AiGatewayService>();
builder.Services.AddScoped<IAccessorClient, AccessorClient>();
builder.Services.AddScoped<IEngineClient, EngineClient>();

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
app.UseCors("AllowAll");
app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();
app.MapManagerEndpoints();
app.MapAiEndpoints();

app.MapHub<NotificationHub>("/notificationHub");

app.Run();
