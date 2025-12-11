namespace StorageAccessor.Endpoints;

public static class StorageEndpoints
{
    public static void MapStorageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("storage-accessor")
            .WithTags("Storage");
    }
}
