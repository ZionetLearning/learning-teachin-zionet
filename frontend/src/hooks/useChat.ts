import { useState } from "react";
import { useSendChatMessage } from "@/api/chat";
import type { ChatRequest, ChatResponse } from "@/api/chat";

type ChatMessage = {
  role: "user" | "assistant";
  text: string;
};

export const useChat = () => {
  const [threadId, setThreadId] = useState<string | undefined>(undefined);
  const [messages, setMessages] = useState<ChatMessage[]>([]);

  const { mutate: sendChatMessage, isPending } = useSendChatMessage();

  const sendMessage = (
    userText: string,
    onAssistantMessage?: (text: string) => void
  ) => {
    const payload: ChatRequest = {
      userMessage: userText,
      threadId: threadId || "123456789",
      chatType: "default",
    };

    setMessages((prev) => [...prev, { role: "user", text: userText }]);

    sendChatMessage(payload, {
      onSuccess: (data: ChatResponse) => {
        setThreadId(data.threadId);

        if (onAssistantMessage) {
          // Don't add to messages yet — let animation handle it
          onAssistantMessage(data.assistantMessage);
        } else {
          setMessages((prev) => [
            ...prev,
            { role: "assistant", text: data.assistantMessage },
          ]);
        }
      },
    });
  };

  return {
    messages: messages.map((m) => ({
      role: m.role,
      content: m.text, // shape it to { role, content } for ChatSh
    })),
    sendMessage,
    loading: isPending,
    threadId,
    setMessages, // Expose this so you can manually add messages from animation
  };
};
