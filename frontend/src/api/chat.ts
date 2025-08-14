import { useMutation } from "@tanstack/react-query";
import { useQueryClient } from "@tanstack/react-query";
import axios from "axios";
import { toast } from "react-toastify";

export type ChatRequest = {
  userMessage: string;
  threadId?: string;
  chatType?: "default" | string;
};

export type ChatResponse = {
  threadId: string;
  assistantMessage: string;
};

export const useSendChatMessage = () => {
  const queryClient = useQueryClient();

  return useMutation<ChatResponse, Error, ChatRequest>({
    mutationFn: async ({
      userMessage,
      threadId = "123456789",
      chatType = "default",
    }) => {
      const response = await axios.post<ChatResponse>(
        //local server endpoint URL:
        // "http://localhost:5280/chat",
        //cloud server endpoint URL:
        "https://teachin.westeurope.cloudapp.azure.com/api/dev/chat",
        {
          userMessage,
          threadId,
          chatType,
        }
      );

      return response.data;
    },

    onSuccess: (data) => {
      toast.success("Message sent successfully");
      queryClient.invalidateQueries({ queryKey: ["chat", data.threadId] });
    },

    onError: (error) => {
      console.error("Failed to send chat message:", error);
      toast.error("Failed to send message. Please try again.");
    },
  });
};
