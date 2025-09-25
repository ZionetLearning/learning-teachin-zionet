using AutoMapper;
using DotQueue;
using Manager.Models.Notifications;
using Manager.Models.QueueMessages;

internal sealed class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<FrameKind, StreamEventStage>()
            .ConvertUsing(static src => ConvertFrameKindToStreamEventStage(src));
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