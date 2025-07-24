import { useState, useEffect, useCallback, useRef } from "react";
import type { Message, MessageContext, ChatError } from "../types";
import { MessageServiceImpl } from "../services";

export interface UseChatReturn {
  messages: Message[];
  isLoading: boolean;
  error?: string;
  sendMessage: (content: string, context?: MessageContext) => Promise<void>;
  clearMessages: () => void;
  retryLastMessage: () => Promise<void>;
  isInitialized: boolean;
}

export function useChat(): UseChatReturn {
  const [messages, setMessages] = useState<Message[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | undefined>();
  const [isInitialized, setIsInitialized] = useState(false);
  const [lastMessageContent, setLastMessageContent] = useState<{
    content: string;
    context?: MessageContext;
  } | null>(null);

  const messageServiceRef = useRef<MessageServiceImpl | null>(null);

  useEffect(() => {
    const initializeService = async () => {
      try {
        if (!messageServiceRef.current) {
          messageServiceRef.current = new MessageServiceImpl();

          const unsubscribe = messageServiceRef.current.subscribeToMessages(
            (updatedMessages) => {
              setMessages(updatedMessages);
              setIsInitialized(true);
            },
          );

          return unsubscribe;
        }
      } catch (err) {
        const errorMessage =
          err instanceof Error
            ? err.message
            : "Failed to initialize chat service";
        setError(errorMessage);
        console.error("Chat initialization error:", err);
      }
    };

    initializeService();
  }, []);

  const handleError = useCallback((err: unknown, operation: string) => {
    let errorMessage: string;
    let errorType: ChatError["type"] = "network";

    if (err instanceof Error) {
      errorMessage = err.message;

      if (err.message.includes("network") || err.message.includes("fetch")) {
        errorType = "network";
      } else if (err.message.includes("validation")) {
        errorType = "validation";
      } else if (err.message.includes("render")) {
        errorType = "rendering";
      } else if (err.message.includes("context")) {
        errorType = "context";
      }
    } else {
      errorMessage = `Failed to ${operation}`;
    }

    const chatError: ChatError = {
      type: errorType,
      message: errorMessage,
      details: err,
      timestamp: new Date(),
    };

    setError(chatError.message);
    console.error(`Chat ${operation} error:`, chatError);
  }, []);

  const sendMessage = useCallback(
    async (content: string, context?: MessageContext) => {
      if (!messageServiceRef.current) {
        handleError(new Error("Chat service not initialized"), "send message");
        return;
      }

      if (!content.trim()) {
        handleError(
          new Error("Message content cannot be empty"),
          "send message",
        );
        return;
      }

      setLastMessageContent({ content, context });
      setIsLoading(true);
      setError(undefined);

      try {
        await messageServiceRef.current.sendMessage(content, context);
      } catch (err) {
        handleError(err, "send message");
      } finally {
        setIsLoading(false);
      }
    },
    [handleError],
  );

  const retryLastMessage = useCallback(async () => {
    if (!lastMessageContent) {
      handleError(new Error("No message to retry"), "retry message");
      return;
    }

    await sendMessage(lastMessageContent.content, lastMessageContent.context);
  }, [lastMessageContent, sendMessage, handleError]);

  const clearMessages = useCallback(() => {
    try {
      if (messageServiceRef.current) {
        messageServiceRef.current.clearMessages();
      }
      setError(undefined);
      setLastMessageContent(null);
    } catch (err) {
      handleError(err, "clear messages");
    }
  }, [handleError]);

  return {
    messages,
    isLoading,
    error,
    sendMessage,
    clearMessages,
    retryLastMessage,
    isInitialized,
  };
}
