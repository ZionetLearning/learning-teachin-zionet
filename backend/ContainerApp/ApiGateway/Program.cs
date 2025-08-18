using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1) Load Ocelot config
builder.Configuration
       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
       .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// 2) Register Ocelot
builder.Services.AddOcelot();

// (Optional) if you added App Insights:
// builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// 3) Standard middleware
app.UseAuthorization();

app.UseWebSockets();

// 4) Hook Ocelot into the pipeline
await app.UseOcelot();
await app.RunAsync();
