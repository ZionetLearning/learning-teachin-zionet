using AutoMapper;
using DotQueue;
using Manager.Models.UserGameConfiguration;
using Manager.Models.Notifications;
using Manager.Models.QueueMessages;
using Manager.Models.Achievements;
using Manager.Services.Clients.Accessor.Models.Achievements;

internal sealed class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<FrameKind, StreamEventStage>()
            .ConvertUsing(static src => ConvertFrameKindToStreamEventStage(src));

        CreateMap<UserNewGameConfig, UserGameConfig>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore());

        // Achievement mappings
        CreateMap<AchievementAccessorModel, AchievementDto>()
            .ForMember(dest => dest.IsUnlocked, opt => opt.Ignore())
            .ForMember(dest => dest.UnlockedAt, opt => opt.Ignore());
    }

    private static StreamEventStage ConvertFrameKindToStreamEventStage(FrameKind src)
    {
        return src switch
        {
            FrameKind.First => StreamEventStage.First,
            FrameKind.Chunk => StreamEventStage.Chunk,
            FrameKind.Last => StreamEventStage.Last,
            FrameKind.Heartbeat => StreamEventStage.Heartbeat,
            FrameKind.Error => StreamEventStage.Error,
            _ => throw new NonRetryableException($"Unknown FrameKind value: {src}")
        };
    }
}