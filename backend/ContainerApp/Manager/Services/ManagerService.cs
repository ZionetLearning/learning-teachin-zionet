using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json.Nodes;

namespace Manager.Services;

public class ManagerService : IManagerService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ManagerService> _logger;
    private readonly DaprClient _daprClient;

    public ManagerService(IConfiguration configuration, ILogger<ManagerService> logger, DaprClient daprClient)
    {
        _configuration = configuration;
        _logger = logger;
        _daprClient = daprClient;
    }

    public async Task<bool> UpdateTaskName(int id, string newTaskName)
    {
        _logger.LogInformation($"Inside {nameof(UpdateTaskName)}");
        try
        {
            if (id <= 0 || string.IsNullOrEmpty(newTaskName)) {
                _logger.LogError("Invalid input: id or task name is null or empty.");
                return false;
            }

            await _daprClient.InvokeBindingAsync(
                "emailqueue", 
                "create", 
                new
                {
                    Id = id,
                    Name = newTaskName
                });

            _logger.LogInformation("Task name update sent to queue.");

            return true;

            }
        catch (Exception e)
        {
            _logger.LogError(e, "Error inside the UpdateUserEmail");
            return false;
        }
    }





    public async Task<bool> DeleteTask(int id)
    {
        _logger.LogInformation($"Inside {nameof(DeleteTask)}");
        try
        {
            if (id <= 0)
            {
                _logger.LogError("Invalid input.");
                return false;
            }

            await _daprClient.InvokeMethodAsync(
                HttpMethod.Delete,
                "accessor", 
                $"user/{id}");

            _logger.LogInformation($"{nameof(DeleteTask)}: task with id: {id} has been deleted");

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error inside the DeleteUser");
            return false;
        }
    }
    
}

