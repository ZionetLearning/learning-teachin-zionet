using Dapr.Client;
using TodoManager.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Manager.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TodoController : ControllerBase
    {
        private readonly ILogger<TodoController> _logger;
        private readonly DaprClient _daprClient;

        public TodoController(ILogger<TodoController> logger, DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        [HttpGet("/todos")]
        public async Task<ActionResult<List<Todo>>> GetAllTodosAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all todos");

                var result = await _daprClient.InvokeMethodAsync<List<Todo>>(HttpMethod.Get, "todoaccessor", "/todos");

                if (result is null)
                {
                    _logger.LogInformation("Request from acessor service return empty list of todos");
                    return Ok(new List<Todo>());
                }

                _logger.LogInformation("List of todos successfully retrieved from accessor service");
                return Ok(result);
            }
            catch (InvocationException ie) when (ie.InnerException is HttpRequestException { StatusCode: System.Net.HttpStatusCode.NotFound })
            {
                _logger.LogInformation("No todos found");
                return Ok(new List<Todo>());
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to get todos, ex: {ex}", ex.Message);
                return Problem("Failed to get todos");
            }
        }

        [HttpGet("/todo/{id}")]
        public async Task<ActionResult<Todo>> GetTodoByIdAsync(
            [FromRoute] string id)
        {
            try
            {
                var result = await _daprClient.InvokeMethodAsync<Todo>(HttpMethod.Get, "todoaccessor", $"/todo/{id}");

                if (result is null)
                {
                    _logger.LogInformation("Request from acessor service return empty list of todos");
                    return NotFound("todos not found");
                }
                else
                {
                    _logger.LogInformation("List of todos successfully retrieved from accessor service");
                    return Ok(result);
                }
            }
            catch (InvocationException ie) when (ie.InnerException is HttpRequestException httpEx && httpEx.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Todo {TodoId} not found (404)", id);
                return NotFound("Todo not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Problem(ex.Message);
            }
        }

        [HttpPost("/todo")]
        public async Task<ActionResult<string>> AddTodoToQueueAsync(Todo todo)
        {
            try
            {
                var command = new TodoCommand
                {
                    Action = TodoCommandAction.Create,
                    Todo = todo
                };
                await _daprClient.InvokeBindingAsync("todoqueue", "create", command);
                _logger.LogInformation("Sent create command for todo id {TodoId}", todo.Id);
                return Ok($"Create command sent for Todo id {todo.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to add new todo, ex: {ex}", ex.Message);
                return Problem("Failed to add new todo");
            }
        }

        [HttpPut("/todo")]
        public async Task<ActionResult<string>> AddUpdateTodoToQueueAsync(Todo todo)
        {
            try
            {
                var command = new TodoCommand
                {
                    Action = TodoCommandAction.Update,
                    Todo = todo
                };
                await _daprClient.InvokeBindingAsync("todoqueue", "create", command);
                _logger.LogInformation("Sent update command for todo id {TodoId}", todo.Id);
                return Ok($"Update command sent for Todo id {todo.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update todo, ex: {ex}", ex.Message);
                return Problem("Failed to update todo");
            }
        }

        [HttpDelete("/todo/{id}")]
        public async Task<ActionResult<string>> AddDeleteTodoToQueueAsync([FromRoute] string id)
        {
            try
            {
                var command = new TodoCommand
                {
                    Action = TodoCommandAction.Delete,
                    Id = id
                };
                await _daprClient.InvokeBindingAsync("todoqueue", "create", command);
                _logger.LogInformation("Sent delete command for todo id {TodoId}", id);
                return Ok($"Delete command sent for Todo id {id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to delete todo, ex: {ex}", ex.Message);
                return Problem("Failed to delete todo");
            }
        }

        [HttpPost("/todomanagercallbackqueue")]
        public async Task<IActionResult> HandleCallback([FromBody] JsonElement data)
        {
            try
            {
                Todo? todo = null;
                if (data.ValueKind == JsonValueKind.String)
                {
                    string? id = data.GetString();
                    if (string.IsNullOrEmpty(id))
                    {
                        _logger.LogError("Received empty id in callback.");
                        return Problem("Empty id payload.");
                    }
                    todo = new Todo { Id = id };
                }
                else if (data.ValueKind == JsonValueKind.Object)
                {
                    todo = JsonSerializer.Deserialize<Todo>(data, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                }

                if (todo == null || string.IsNullOrEmpty(todo.Id))
                {
                    _logger.LogError("No valid todo data received in callback.");
                    return Problem("Empty or invalid payload.");
                }
                _logger.LogInformation("Received callback for Todo {TodoId}", todo.Id);

                var clientResponse = new ClientResponse
                {
                    Message = "Processing completed for Todo " + todo.Id,
                    Todo = todo
                };

                await _daprClient.InvokeBindingAsync("clientresponsequeue", "create", clientResponse);

                _logger.LogInformation("Forwarded callback to clientresponsequeue for Todo {TodoId}", todo.Id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error processing callback binding: {ex}", ex.Message);
                return Problem("Error processing callback binding");
            }
        }
    }
}