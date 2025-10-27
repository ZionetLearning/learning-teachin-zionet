/// <reference types="vite/client" />
import { useQuery } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import { isAxiosError } from "axios";
import { toast } from "react-toastify";
import { EventType } from "@app-providers/types";
import { useSignalR } from "@student/hooks";
import {
  SendMessageRequest,
  AIChatStreamResponse,
  Chat,
  ChatHistory,
  StreamMetaEvent,
  StreamStage,
} from "@student/types";

export const useSendChatMessageStream = () => {
  const AI_BASE_URL = import.meta.env.VITE_AI_URL!;
  const { waitForStream, status } = useSignalR();

  const startStream = async (
    {
      userMessage,
      threadId = crypto.randomUUID(),
      chatType = "default",
      userId,
    }: SendMessageRequest,
    onDelta: (delta: string) => void,
    onCompleted: (final: AIChatStreamResponse) => void,
    onMeta?: (evt?: StreamMetaEvent) => void,
  ) => {
    // Check SignalR connection
    if (status !== "connected") {
      throw new Error(`SignalR not connected. Status: ${status}`);
    }

    try {
      // Start the request
      const { data } = await axios.post<{ requestId: string }>(
        `${AI_BASE_URL}/chat`,
        { userMessage, threadId, chatType, userId },
      );

      const requestId = data.requestId;

      // Use waitForStream which creates and manages the stream
      const streamMessages = waitForStream<AIChatStreamResponse>(
        EventType.ChatAiAnswer,
        requestId,
        (msg) => {
          const payload = msg?.payload ?? {};
          const stage = payload?.stage as StreamStage | undefined; // "Tool" | "Model" | "Final"
          const toolCall = (payload?.toolCall ?? null) as string | null;
          const isFinal = !!payload?.isFinal;
          const delta = payload?.delta;

          // 1) Emit meta
          onMeta?.({ stage, toolCall, isFinal });

          // 2) if final, complete.
          if (isFinal) {
            onCompleted(payload as AIChatStreamResponse);
            return; //  prevent duplicate text
          }

          // 3) model chunks only while not final
          if (typeof delta === "string" && delta.length > 0) {
            onDelta(delta);
          }
        },
      );

      streamMessages.catch((error) => {
        console.error("Stream error:", error);
      });
    } catch (error) {
      console.error("Error starting stream:", error);
      throw error;
    }
  };

  return { startStream };
};

export const useGetAllChats = (userId: string) => {
  const AI_BASE_URL = import.meta.env.VITE_AI_URL!;

  return useQuery<Chat[], Error>({
    queryKey: ["chats", userId],
    queryFn: async () => {
      try {
        const { data } = await axios.get<{ chats: Chat[] }>(
          `${AI_BASE_URL}/chats/${userId}`,
        );

        return data.chats || [];
      } catch (error: unknown) {
        if (isAxiosError(error) && error.response?.status === 404) {
          return [];
        }
        console.error("Failed to fetch chats:", error);
        toast.error("Failed to load chats. Please try again.");
        throw error;
      }
    },
    enabled: !!userId,
  });
};

export const useGetChatHistory = (chatId: string, userId: string) => {
  const AI_BASE_URL = import.meta.env.VITE_AI_URL!;

  return useQuery<ChatHistory, Error>({
    queryKey: ["chat", chatId, userId],
    queryFn: async () => {
      try {
        const { data } = await axios.get<ChatHistory>(
          `${AI_BASE_URL}/chat/${chatId}/${userId}`,
        );
        return data;
      } catch (error: unknown) {
        if (isAxiosError(error) && error.response?.status === 404) {
          return {
            chatId: chatId,
            name: "New Chat",
            chatType: "default",
            messages: [],
          };
        }
        console.error("Failed to fetch chat history:", error);
        toast.error("Failed to load chat history. Please try again.");
        throw error;
      }
    },
    enabled: !!chatId && !!userId,
  });
};
