using Accessor.Endpoints;
using Accessor.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers().AddDapr();

builder.Services.AddScoped<IAccessorService, AccessorService>();


builder.Services.AddMemoryCache();

// Register a default cache policy globally
builder.Services.AddSingleton<MemoryCacheEntryOptions>(_ =>
    new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    });



// Add database context
builder.Services.AddDbContext<AccessorDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var startupService = scope.ServiceProvider.GetRequiredService<IAccessorService>();
    await startupService.InitializeAsync(); 
}



app.UseCloudEvents();
app.MapSubscribeHandler();


app.MapAccessorEndpoints();

app.Run();
