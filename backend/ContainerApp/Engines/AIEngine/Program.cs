using AIEngine.Configuration;
using AIEngine.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Services.Configure<AISettings>(
    builder.Configuration.GetSection("AISettings"));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
}

app.MapAIEndpoints();


app.Run();
