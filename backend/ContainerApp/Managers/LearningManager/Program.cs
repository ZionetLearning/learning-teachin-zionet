using LearningManger.Configuration;
using LearningManger.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<LearningSettings>(
    builder.Configuration.GetSection("LearningSettings"));

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{

}

app.UseCors();

app.MapLearningEndpoints();

app.Run();
