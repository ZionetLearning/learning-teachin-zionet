/// <reference types="vite/client" />
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import { isAxiosError } from "axios";
import { toast } from "react-toastify";
import { EventType } from "@app-providers/types";
import { useSignalR } from "@student/hooks";
import { 
  SendMessageRequest, 
  SendMessageResponse, 
  Chat, 
  ChatHistory 
} from "@student/types";

export const useSendChatMessage = () => {
  const AI_BASE_URL = import.meta.env.VITE_AI_URL!;
  const queryClient = useQueryClient();
  const { waitForResponse } = useSignalR();

  return useMutation<SendMessageResponse, Error, SendMessageRequest>({
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

      const aiResponse = await waitForResponse<SendMessageResponse>(
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

export const useGetAllChats = (userId: string) => {
  const AI_BASE_URL = import.meta.env.VITE_AI_URL!;

  return useQuery<Chat[], Error>({
    queryKey: ["chats", userId],
    queryFn: async () => {
      try {
        const { data } = await axios.get<{chats: Chat[]}>(
          `${AI_BASE_URL}/chats/${userId}`
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
          `${AI_BASE_URL}/chat/${chatId}/${userId}`
        );
        return data;
      } catch (error: unknown) {
       
        if (isAxiosError(error) && error.response?.status === 404) {
          return {
            chatId: chatId,
            name: "New Chat",
            chatType: "default",
            messages: []
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
