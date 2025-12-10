namespace DatabaseAccessor.Endpoints
{
    public static class DatabaseEndpoints
    {
        // Demo file
        public static void MapDatabaseEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("database-accessor")
                .WithTags("Database");

        }
    }
}
