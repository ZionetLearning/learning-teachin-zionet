/// <reference types="vite/client" />
import { useMutation, useQueryClient } from "@tanstack/react-query";
import axios from "axios";
import { toast } from "react-toastify";
import { EventType } from "@app-providers/types";
import { useSignalR } from "@student/hooks";

export type ChatRequest = {
  userMessage: string;
  threadId?: string;
  chatType?: "default" | string;
  userId: string; // needed for correlation
};

// need to fix backend to send camelCase
export type ChatResponse = {
  RequestId: string;
  AssistantMessage?: string;
  ChatName: string;
  Status: number;
  ThreadId: string;
};

export const useSendChatMessage = () => {
  const AI_BASE_URL = import.meta.env.VITE_AI_URL!;
  const queryClient = useQueryClient();
  const { waitForResponse } = useSignalR();

  return useMutation<ChatResponse, Error, ChatRequest>({
    mutationFn: async ({
      userMessage,
      threadId = crypto.randomUUID(),
      chatType = "default",
      userId,
    }) => {
      // 1. Send request to backend → get requestId
      const { data } = await axios.post<{ requestId: string }>(
        `${AI_BASE_URL}/chat`,
        {
          userMessage,
          threadId,
          chatType,
          userId,
        },
      );

      const requestId = data.requestId;

      // 2. Wait for SignalR event carrying the full AIChatResponse
      const aiResponse = await waitForResponse<ChatResponse>(
        EventType.ChatAiAnswer,
        requestId,
      );
      return aiResponse;
    },

    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ["chat", data.ThreadId] });
    },

    onError: (error) => {
      console.error("Failed to send chat message:", error);
      toast.error("Failed to send message. Please try again.");
    },
  });
};
