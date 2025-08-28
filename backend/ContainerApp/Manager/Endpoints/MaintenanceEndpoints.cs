using Manager.Services.Clients.Accessor;

namespace Manager.Endpoints;

public static class MaintenanceEndpoints
{
    public static void MapMaintenanceEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/internal/maintenance").WithTags("Maintenance");

        group.MapPost("/refresh-sessions/cleanup", async (
            IAccessorClient accessorClient,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("Maintenance.RefreshSessionsCleanup");

            try
            {
                var deleted = await accessorClient.CleanupRefreshSessionsAsync(ct);
                logger.LogInformation("Cleanup done; deleted={Deleted}", deleted);
                return Results.Ok(new { deleted });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Cleanup failed");
                return Results.Problem("Cleanup failed");
            }
        })
        .WithName("Maintenance_RefreshSessionsCleanup");
    }
}
