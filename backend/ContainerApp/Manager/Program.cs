using Azure.Messaging.ServiceBus;
using Manager.Constants;
using Manager.Endpoints;
using Manager.Hubs;
using Manager.Messaging;
using Manager.Models;
using Manager.Services;
using Manager.Services.Clients;
using Scalar.AspNetCore;
using Azure.Messaging.ServiceBus.Administration;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<AiSettings>(builder.Configuration.GetSection("Ai"));

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

// --- Service Bus clients ---
builder.Services.AddSingleton(sp =>
  new ServiceBusClient(builder.Configuration["ServiceBus:ConnectionString"]));

//admin client used only to *wait* for the queue to exist
builder.Services.AddSingleton(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var cs = cfg["ServiceBus:ConnectionString"] ?? cfg.GetConnectionString("ServiceBus");
    return new ServiceBusAdministrationClient(cs!);
});

builder.Services.AddQueue<Message, ManagerQueueHandler>(
    QueueNames.ManagerCallbackQueue,
    settings =>
    {
        settings.MaxConcurrentCalls = 5;
        settings.PrefetchCount = 10;
        settings.ProcessingDelayMs = 200;
        settings.MaxRetryAttempts = 3;
        settings.RetryDelaySeconds = 2;
    });

// This is required for the Scalar UI to have an option to setup an authentication token
builder.Services.AddOpenApi(
    "v1",
    options =>
    {
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    }
);

var app = builder.Build();
app.UseCors("AllowAll");
app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();
app.MapManagerEndpoints();
app.MapAiEndpoints();

if (env.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Manager API";
        options.Theme = ScalarTheme.BluePlanet;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
        options.ShowSidebar = true;
        options.PersistentAuthentication = true;
        // here we can setup a default token
        //options.AddPreferredSecuritySchemes("Bearer")
        // .AddHttpAuthentication("Bearer", auth =>
        // {
        //     auth.Token = "Some Auth Token...";
        // });

    });
}

app.MapHub<NotificationHub>("/notificationHub");

app.Run();
