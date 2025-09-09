using AutoMapper;
using Accessor.Models.Prompts;

internal sealed class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<PromptModel, PromptResponse>();
    }
}
