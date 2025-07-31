using ToDoAccessor.Services;
using ToDoAccessor.Models;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

namespace ToDoAccessor.Controllers
{
    public static class ToDoController
    {
        public static void MapTodoController(this WebApplication app)
        {
            app.MapGet("/todos", async ([FromServices] ITodoService todoService) =>
            {
                var todos = await todoService.GetAllTodosAsync();
                return Results.Ok(todos);
            });

            app.MapGet("/todo/{id}", async ([FromRoute] string id, [FromServices] ITodoService todoService) =>
            {
                var todo = await todoService.GetTodoByIdAsync(id);
                return todo != null ? Results.Ok(todo) : Results.NotFound($"Todo not found for ID: {id}");
            });

            app.MapPost(Queues.TodoQueue, async (
                [FromBody] TodoCommand command,
                [FromServices] ITodoService todoService,
                [FromServices] DaprClient daprClient,
                [FromServices] ILogger<Program> logger) =>
            {
                try
                {
                    switch (command.Action)
                    {
                        case TodoCommandAction.Create:
                            if (command.Todo is null)
                                return Results.BadRequest("Todo object is required for create.");
                            await todoService.AddTodoAsync(command.Todo);
                            await daprClient.InvokeBindingAsync(Queues.TodoManagerCallbackQueue, "create", command.Todo);
                            logger.LogInformation("Created todo with id {TodoId}", command.Todo.Id);
                            return Results.Ok($"Todo created with id {command.Todo.Id}");

                        case TodoCommandAction.Update:
                            if (command.Todo is null)
                                return Results.BadRequest("Todo object is required for update.");
                            bool updated = await todoService.UpdateTodoAsync(command.Todo.Id, command.Todo);
                            await daprClient.InvokeBindingAsync(Queues.TodoManagerCallbackQueue, "create", command.Todo);
                            logger.LogInformation("Updated todo with id {TodoId}", command.Todo.Id);
                            return updated
                                ? Results.Ok($"Todo updated with id {command.Todo.Id}")
                                : Results.NotFound($"Todo not found for id {command.Todo.Id}");

                        case TodoCommandAction.Delete:
                            if (string.IsNullOrEmpty(command.Id))
                                return Results.BadRequest("Id is required for delete.");
                            bool deleted = await todoService.DeleteTodoAsync(command.Id);
                            await daprClient.InvokeBindingAsync(Queues.TodoManagerCallbackQueue, "create", command.Id);
                            logger.LogInformation("Deleted todo with id {TodoId}", command.Id);
                            return deleted
                                ? Results.Ok($"Todo deleted with id {command.Id}")
                                : Results.NotFound($"Todo not found for id {command.Id}");

                        default:
                            return Results.BadRequest("Invalid command action.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing todo command.");
                    return Results.Problem("Error processing todo command.");
                }
            });
        }
    }
}
