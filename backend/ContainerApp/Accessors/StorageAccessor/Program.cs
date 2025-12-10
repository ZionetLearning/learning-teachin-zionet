using StorageAccessor.Configuration;
using StorageAccessor.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<StorageSettings>(
    builder.Configuration.GetSection("StorageSettings"));


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

app.MapStorageEndpoints();

app.Run();
