using Manager.Endpoints;
using Manager.Hubs;
using Manager.Services;
using Manager.Services.Clients;


var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();
app.UseCors("AllowAll");
app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();
app.MapManagerEndpoints();
app.MapAiEndpoints();

app.MapHub<NotificationHub>("/notificationHub");

app.Run();
