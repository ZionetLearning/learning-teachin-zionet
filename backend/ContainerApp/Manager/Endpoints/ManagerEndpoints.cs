using Manager.Services;
using Microsoft.Extensions.Logging;


public static class ManagerEndpoints
{
    
    public static void MapManagerEndpoints(this WebApplication app)
    {





        #region HTTP PUT

        app.MapPut("/manager/{id}/{name}", async (int id, 
            string name, ILogger<ManagerService> logger, 
            IManagerService manager) =>
        {
            logger.LogInformation("Get account by id from account manager with {id}", id);

            var success = await manager.UpdateTaskName(id, name);
            return success ? Results.Ok("Task name updated") : Results.NotFound("Task not found");
        });

        #endregion






        #region HTTP DELETE


        app.MapDelete("/manager/{id}", async (int id, 
            ILogger<ManagerService> logger, 
            IManagerService manager) =>
        {
            logger.LogInformation("Delete task with id {id}", id);
            var success = await manager.DeleteTask(id);
            return success ? Results.Ok("Task deleted") : Results.NotFound("Task not found");
        });


        #endregion



    }


}
