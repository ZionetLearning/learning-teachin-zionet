using Microsoft.AspNetCore.Mvc;
using NotificationManager.Services;
using NotificationManager.Models;
using System.Text.Json.Serialization;
using System.Text.Json;
using Dapr.Client;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<SignalRService>()
#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
    .AddHostedService(sp => sp.GetService<SignalRService>()!)
#pragma warning restore CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
#pragma warning restore CS8631 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match constraint type.
    .AddSingleton<IHubContextStore>(sp => sp.GetService<SignalRService>()!)
    .AddDaprClient();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddControllers().AddDapr().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapPost("/negotiate", async (
    HttpContext context,
    [FromServices] IHubContextStore hubStore) =>
{
    var userId = context.User?.Identity?.Name ?? "anonymous";

    var hubContext = hubStore.TodoNotificationsHubContext;
    if (hubContext == null)
    {
        return Results.Problem("SignalR hub context is not ready.");
    }

    // Negotiate a new connection
    var negotiateResponse = await hubContext.NegotiateAsync(new() {
        UserId = userId,
        EnableDetailedErrors = true
    });

    return Results.Json(new
    {
        url = negotiateResponse.Url,
        accessToken = negotiateResponse.AccessToken
    });
});

app.MapPost("/clientresponsequeue", async (
    [FromBody] ClientResponse clientResponse,
    [FromServices] DaprClient daprClient,
    [FromServices] ILogger<Program> logger) =>
{
    try
    {
        var payload = JsonSerializer.SerializeToElement(clientResponse);
        await PublishMessageToSignalRAsync(
            clientResponse.Todo.Id,
            "todoNotification",
            payload,
            daprClient,
            logger);
        return Results.Ok("Notification sent.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error sending notification.");
        return Results.BadRequest("Failed to send notification.");
    }
});

app.Run();

async Task PublishMessageToSignalRAsync(
    string? callerId,
    string actionName,
    System.Text.Json.JsonElement payload,
    DaprClient daprClient,
    ILogger logger)
{
    try
    {
        var argument = new Argument
        {
            Sender = "TodoApp",
            Text = payload
        };

        SignalRMessage message = new()
        {
            UserId = callerId,
            Target = actionName,
            Arguments = [argument]
        };

        var metadata = new Dictionary<string, string?>();
        //if (!string.IsNullOrEmpty(callerId) && callerId != "all")
        //{
        //    metadata["user"] = callerId;
        //}

        logger.LogInformation("Publishing message to SignalR with payload: {Payload}", payload);

        await daprClient.InvokeBindingAsync("clientcallback", "create", message, metadata);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to publish message to SignalR.");
    }
}