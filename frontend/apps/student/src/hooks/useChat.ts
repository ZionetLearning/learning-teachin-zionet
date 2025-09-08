import { useState, useMemo } from "react";
import { useSendChatMessage } from "@student/api";
import { useAuth } from "@app-providers/auth";
import { decodeJwtPayload } from "@app-providers/auth/utils";
import type { ChatRequest, ChatResponse } from "@student/api";

export type ChatPosition = "left" | "right";
export type ChatSender = "user" | "system";

export interface ChatMessage {
  position: ChatPosition;
  type: "text";
  sender: ChatSender;
  text: string;
  date: Date;
}

export const useChat = () => {
  const [threadId, setThreadId] = useState<string | undefined>(undefined);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const { accessToken } = useAuth();

  const {
    mutate: sendChatMessage,
    mutateAsync: sendChatMessageAsync,
    isPending,
  } = useSendChatMessage();

  const userId = useMemo(() => {
    if (!accessToken) return null;
    const payload = decodeJwtPayload(accessToken);
    if (!payload) return null;
    return payload.userId as string;
  }, [accessToken]);

  const pushUser = (text: string) => {
    const userMsg: ChatMessage = {
      position: "right",
      type: "text",
      sender: "user",
      text,
      date: new Date(),
    };
    setMessages((prev) => [...prev, userMsg]);
  };

  const pushAssistant = (text: string) => {
    const aiMsg: ChatMessage = {
      position: "left",
      type: "text",
      sender: "system",
      text,
      date: new Date(),
    };
    setMessages((prev) => [...prev, aiMsg]);
  };

  const sendMessage = (userText: string) => {
    if (!userText.trim() || !userId) return;

    const payload: ChatRequest = {
      userMessage: userText,
      threadId: threadId || crypto.randomUUID(),
      chatType: "default",
      userId: userId
    };

    pushUser(userText);

    // call API
    sendChatMessage(payload, {
      onSuccess: (data: ChatResponse) => {
        setThreadId(data.threadId);


        const aiText = data.assistantMessage ?? "";
        const aiMsg: ChatMessage = {
          position: "left",
          type: "text",
          sender: "system",
          text: aiText,
          date: new Date(),
        };
        setMessages((prev) => [...prev, aiMsg]);
      },
    });
  };

  const sendMessageAsync = async (userText: string): Promise<string> => {
    if (!userText.trim() || !userId) return "";

    const payload: ChatRequest = {
      userMessage: userText,
      threadId: threadId || crypto.randomUUID(),
      chatType: "default",
      userId: userId
    };

    pushUser(userText);

    const data = await sendChatMessageAsync(payload);
    setThreadId(data.threadId);
    pushAssistant(data.assistantMessage ?? "");
    return data.assistantMessage ?? "";
  };

  return {
    messages,
    sendMessage,
    sendMessageAsync,
    loading: isPending,
    threadId,
    setMessages,
  };
};
