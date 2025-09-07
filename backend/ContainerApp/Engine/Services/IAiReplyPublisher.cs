﻿using Engine.Models.Chat;
using Engine.Models.QueueMessages;
using Engine.Models.Sentences;

namespace Engine.Services;

public interface IAiReplyPublisher
{
    Task SendReplyAsync(UserContextMetadata chatMetadata, EngineChatResponse response, CancellationToken ct = default);
    Task SendGeneratedMessagesAsync(string userId, SentenceResponse response, CancellationToken ct = default);
}