import { useState, useMemo, useCallback } from "react";
import { useSendChatMessage, useGetAllChats, useGetChatHistory } from "@student/api";
import { useAuth } from "@app-providers/auth";
import { decodeJwtPayload } from "@app-providers/auth/utils";
import type { SendMessageRequest, SendMessageResponse } from "@student/types";

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
  const [shouldLoadHistory, setShouldLoadHistory] = useState(false);
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

  const {
    data: allChats,
    isLoading: isLoadingChats,
    refetch: refetchChats,
  } = useGetAllChats(userId || "");


  const {
    data: chatHistory,
    isLoading: isLoadingHistory,
    refetch: refetchHistory,
  } = useGetChatHistory(
    shouldLoadHistory ? threadId || "" : "", 
    shouldLoadHistory ? userId || "" : ""
  );

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

  const loadChatHistory = (chatId: string) => {
    if (!userId) return;
    setMessages([]);
    setThreadId(chatId);
    setShouldLoadHistory(true);
  };

  const loadHistoryIntoMessages = useCallback(() => {
    if (!chatHistory?.messages) return;
    
    interface ChatHistoryMessage {
      role: "user" | "assistant" | "system";
      text: string;
      createdAt?: string | number | Date | null;
    }

    const convertedMessages: ChatMessage[] = (chatHistory.messages as ChatHistoryMessage[]).map(
      (msg: ChatHistoryMessage): ChatMessage => ({
      position: msg.role === "user" ? "right" : "left",
      type: "text",
      sender: msg.role === "user" ? "user" : "system",
      text: msg.text,
      date: msg.createdAt ? new Date(msg.createdAt) : new Date(),
      })
    );
    
    setMessages(convertedMessages);
    setShouldLoadHistory(false);
  }, [chatHistory?.messages]);

  const startNewChat = () => {
    setThreadId(undefined);
    setMessages([]);
    setShouldLoadHistory(false);
  };

  const sendMessage = (userText: string) => {
    if (!userText.trim() || !userId) return;

    const payload: SendMessageRequest = {
      userMessage: userText,
      threadId: threadId || crypto.randomUUID(),
      chatType: "default",
      userId: userId
    };

    pushUser(userText);

    sendChatMessage(payload, {
      onSuccess: (data: SendMessageResponse) => {
        setThreadId(data.threadId);
        const aiText = data.assistantMessage ?? "";
        pushAssistant(aiText);
        
        if (!threadId) {
          refetchChats();
        }
      },
    });
  };

  const sendMessageAsync = async (userText: string): Promise<string> => {
    if (!userText.trim() || !userId) return "";

    const payload: SendMessageRequest = {
      userMessage: userText,
      threadId: threadId || crypto.randomUUID(),
      chatType: "default",
      userId: userId
    };

    pushUser(userText);

    const data = await sendChatMessageAsync(payload);
    setThreadId(data.threadId);
    pushAssistant(data.assistantMessage ?? "");
    
    if (!threadId) {
      refetchChats();
    }
    
    return data.assistantMessage ?? "";
  };

  return {
    messages,
    sendMessage,
    sendMessageAsync,
    loading: isPending,
    threadId,
    setMessages,
    
    allChats,
    isLoadingChats,
    chatHistory,
    isLoadingHistory,
    loadChatHistory,
    loadHistoryIntoMessages,
    startNewChat,
    
    refetchChats,
    refetchHistory,
  };
};
