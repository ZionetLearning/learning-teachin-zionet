namespace AIEngine.Endpoints;

public static class AIEndpoints
{
    // Demo file
    public static void MapAIEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("ai-engine")
            .WithTags("AI");
    }
}
