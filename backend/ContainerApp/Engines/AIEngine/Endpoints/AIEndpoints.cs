namespace AIEngine.Endpoints;

public static class AIEndpoints
{
    // Demo file
    public static void MapAIEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ai")
            .WithTags("AI");
    }
}
