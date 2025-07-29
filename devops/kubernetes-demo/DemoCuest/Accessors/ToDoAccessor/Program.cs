using ToDoAccessor;
using ToDoAccessor.Services;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Models;
using ToDoAccessor.Controllers;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDaprClient();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});
builder.Services.Configure<TodoCosmosDbSettings>(
    builder.Configuration.GetSection("TodoCosmosDbSettings"));

builder.Services.AddScoped<ITodoService, TodoService>();

var app = builder.Build();

// ✅ Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
    });
}

app.UseAuthorization();
app.MapControllers();

app.UseHttpsRedirection();

app.MapTodoController();

app.Run();
