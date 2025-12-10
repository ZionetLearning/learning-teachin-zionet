namespace LearningManger.Endpoints;

public static class LearningEndpoints
{
    public static void MapLearningEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("learnings-manager")
            .WithTags("Learning");
    }

}
