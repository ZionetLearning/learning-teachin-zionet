import { useState } from "react";
import { useSendChatMessage } from "@/api/chat";
import type { ChatRequest, ChatResponse } from "@/api/chat";

export type ChatPosition = "left" | "right";
export type ChatSender = "user" | "system";

export interface ChatMessage {
  position: ChatPosition;
  type: "text";
  sender: ChatSender;
  text: string;
  date: Date;
}

type OnAssistantMessage = (text: string) => void;

export const useChat = () => {
  const [threadId, setThreadId] = useState<string | undefined>(undefined);
  const [messages, setMessages] = useState<ChatMessage[]>([]);

  const { mutate: sendChatMessage, isPending } = useSendChatMessage();

  const sendMessage = (
    userText: string,
    onAssistantMessage?: OnAssistantMessage,
  ) => {
    if (!userText.trim()) return;

    const payload: ChatRequest = {
      userMessage: userText,
      threadId: threadId || "123456789",
      chatType: "default",
    };

    // push user message
    const userMsg: ChatMessage = {
      position: "right",
      type: "text",
      sender: "user",
      text: userText,
      date: new Date(),
    };
    setMessages((prev) => [...prev, userMsg]);

    // call API
    sendChatMessage(payload, {
      onSuccess: (data: ChatResponse) => {
        setThreadId(data.threadId);

        const aiText = data.assistantMessage;

        if (onAssistantMessage) {
          onAssistantMessage(aiText);
        } else {
          const aiMsg: ChatMessage = {
            position: "left",
            type: "text",
            sender: "system",
            text: aiText,
            date: new Date(),
          };
          setMessages((prev) => [...prev, aiMsg]);
        }
      },
    });
  };

  return {
    messages,
    sendMessage,
    loading: isPending,
    threadId,
    setMessages,
  };
};
