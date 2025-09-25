import { useState, useMemo, useCallback } from "react";
import { useSendChatMessageStream, useGetAllChats, useGetChatHistory } from "@student/api";
import { useAuth } from "@app-providers/auth";
import { decodeJwtPayload } from "@app-providers/auth/utils";
import type { SendMessageRequest, AIChatStreamResponse } from "@student/types";

export type ChatPosition = "left" | "right";
export type ChatSender = "user" | "system";

export interface ChatMessage {
  position: ChatPosition;
  type: "text";
  sender: ChatSender;
  text: string;
  date: Date;
  isTyping?: boolean; // Add typing indicator
}

interface ChatHistoryMessage {
  role: "user" | "assistant" | "system";
  text: string;
  createdAt?: string | number | Date | null;
}

export const useChat = () => {
  const [threadId, setThreadId] = useState<string | undefined>(undefined);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [shouldLoadHistory, setShouldLoadHistory] = useState(false);
  const [isStreaming, setIsStreaming] = useState(false);
  const { accessToken } = useAuth();

  const { startStream } = useSendChatMessageStream();

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

  const loadChatHistory = (chatId: string) => {
    if (!userId) return;
    setMessages([]);
    setThreadId(chatId);
    setShouldLoadHistory(true);
  };

  const loadHistoryIntoMessages = useCallback(() => {
    if (!chatHistory?.messages) return;
    
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
    if (!userText.trim() || !userId || isStreaming) return;

    const payload: SendMessageRequest = {
      userMessage: userText,
      threadId: threadId || crypto.randomUUID(),
      chatType: "default",
      userId: userId
    };

    pushUser(userText);
    setIsStreaming(true);

    // Add initial typing indicator message
    const typingMessage: ChatMessage = {
      position: "left",
      type: "text",
      sender: "system",
      text: "", // Start with empty text
      date: new Date(),
      isTyping: true,
    };
    
    setMessages((prev) => [...prev, typingMessage]);

    let assistantBuffer = "";

    startStream(
      payload,
      (delta: string) => {
        // add partial text
        assistantBuffer += delta;
        // Update the typing message with streamed content
        setMessages((prev) => {
          const updated = [...prev];
          const lastIndex = updated.length - 1;
          const lastMessage = updated[lastIndex];
          
          // Update the last message if it's our typing message
          if (lastMessage?.sender === "system" && lastMessage.isTyping) {
            updated[lastIndex] = { 
              ...lastMessage, 
              text: assistantBuffer,
              isTyping: true // Keep typing indicator while streaming
            };
          }
          
          return updated;
        });
      },
      (final: AIChatStreamResponse) => {
        setThreadId(final.threadId);
        setIsStreaming(false);
        
        // Mark the message as complete (not typing anymore)
        setMessages((prev) => {
          const updated = [...prev];
          const lastIndex = updated.length - 1;
          const lastMessage = updated[lastIndex];
          
          if (lastMessage?.sender === "system" && lastMessage.isTyping) {
            updated[lastIndex] = { 
              ...lastMessage, 
              text: assistantBuffer,
              isTyping: false // Remove typing indicator
            };
          }
          
          return updated;
        });
        
        refetchChats();
      }
    );
  };

  const sendMessageAsync = async (userText: string): Promise<string> => {
    if (!userText.trim() || !userId || isStreaming) return "";

    const payload: SendMessageRequest = {
      userMessage: userText,
      threadId: threadId || crypto.randomUUID(),
      chatType: "default",
      userId: userId
    };

    pushUser(userText);
    setIsStreaming(true);

    // Add initial typing indicator message
    const typingMessage: ChatMessage = {
      position: "left",
      type: "text",
      sender: "system",
      text: "",
      date: new Date(),
      isTyping: true,
    };
    
    setMessages((prev) => [...prev, typingMessage]);

    return new Promise((resolve) => {
      let assistantBuffer = "";

      startStream(
        payload,
        (delta: string) => {
          // add partial text
          assistantBuffer += delta;
          // Update the typing message with streamed content
          setMessages((prev) => {
            const updated = [...prev];
            const lastIndex = updated.length - 1;
            const lastMessage = updated[lastIndex];
            
            if (lastMessage?.sender === "system" && lastMessage.isTyping) {
              updated[lastIndex] = { 
                ...lastMessage, 
                text: assistantBuffer,
                isTyping: true
              };
            }
            
            return updated;
          });
        },
        (final: AIChatStreamResponse) => {
          setThreadId(final.threadId);
          setIsStreaming(false);
          
          // Mark the message as complete
          setMessages((prev) => {
            const updated = [...prev];
            const lastIndex = updated.length - 1;
            const lastMessage = updated[lastIndex];
            
            if (lastMessage?.sender === "system" && lastMessage.isTyping) {
              updated[lastIndex] = { 
                ...lastMessage, 
                text: assistantBuffer,
                isTyping: false
              };
            }
            
            return updated;
          });
          
          if (!threadId) {
            refetchChats();
          }
          resolve(assistantBuffer);
        }
      );
    });
  };

  return {
    messages,
    sendMessage,
    sendMessageAsync,
    loading: isStreaming,
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
