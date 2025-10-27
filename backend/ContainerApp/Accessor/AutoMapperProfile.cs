using Accessor.Models.Games;
using Accessor.Models.Prompts;
using AutoMapper;

internal sealed class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<PromptModel, PromptResponse>();
        CreateMap<GameAttempt, SubmitAttemptResult>();
    }
}
