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

export type ChatResponse = {
  requestId: string;
  assistantMessage?: string;
  chatName: string;
  status: number;
  threadId: string;
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

      const aiResponse = await waitForResponse<ChatResponse>(
        EventType.ChatAiAnswer,
        requestId,
      );
      return aiResponse;
    },

    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ["chat", data.threadId] });
    },

    onError: (error) => {
      console.error("Failed to send chat message:", error);
      toast.error("Failed to send message. Please try again.");
    },
  });
};
