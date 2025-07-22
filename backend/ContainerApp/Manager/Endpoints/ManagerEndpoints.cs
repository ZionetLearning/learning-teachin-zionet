using Manager.Services;
using Microsoft.Extensions.Logging;


public static class ManagerEndpoints
{
    
    public static void MapManagerEndpoints(this WebApplication app)
    {





        #region HTTP PUT

        // PUT /manager/{id}/{email}
        app.MapPut("/manager/{id}/{name}", async (int id, string name, ILogger<Program> logger, IManagerService manager) =>
        {
            logger.LogInformation("Get account by id from account manager with {id}", id);

            var success = await manager.UpdateUserEmail(id, name);
            return success ? Results.Ok("Email updated") : Results.NotFound("User not found");
        });

        #endregion






        #region HTTP DELETE


        // DELETE /manager/{id}
        app.MapDelete("/manager/{id}", async (int id, ILogger<Program> logger, IManagerService manager) =>
        {
            logger.LogInformation("Delete account by id from account manager with {id}", id);
            var success = await manager.DeleteUser(id);
            return success ? Results.Ok("User deleted") : Results.NotFound("User not found");
        });


        #endregion



    }


}
