//using Dapr.Actors.AspNetCore;
using Dapr.Actors.Runtime;
using Dapr.Client;
using Manager.Services;
//using Dapr.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);

// ---- Services ----
builder.Services.AddControllers();

builder.Services.AddControllers().AddDapr();


builder.Services.AddScoped<IManagerService, ManagerService>();


// Add Dapr client
//builder.Services.AddDaprClient(client =>
//{
//    client.UseJsonSerializationOptions(new JsonSerializerOptions
//    {
//        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//        PropertyNameCaseInsensitive = true,
//        WriteIndented = true
//    });
//});



var app = builder.Build();


//app.UseHttpsRedirection();

// ---- Routing ----
app.MapControllers();

app.UseCloudEvents();
app.MapSubscribeHandler();

// Mount the endpoint group of the manager service
app.MapManagerEndpoints();

app.Run();
