using Manager.Services;
using Manager.Endpoints;


var builder = WebApplication.CreateBuilder(args);

// ---- Services ----
builder.Services.AddControllers();

builder.Services.AddControllers().AddDapr();


builder.Services.AddScoped<IManagerService, ManagerService>();


var app = builder.Build();

app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();
app.MapManagerEndpoints();

app.Run();
