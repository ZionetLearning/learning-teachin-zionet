import { useState, useCallback, useRef } from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "@app-providers";
import { useSendChatMessageStream } from "@student/api";
import type { ChatMessage, PageContext } from "../types";

export const useContextAwareChat = (pageContext: PageContext) => {
  const { user } = useAuth();
  const { i18n } = useTranslation();
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const threadIdRef = useRef<string>(crypto.randomUUID());
  const { startStream } = useSendChatMessageStream();

  const sendMessage = useCallback(
    async (text: string) => {
      if (!user?.userId) {
        console.error("User not authenticated");
        return;
      }

      const userMessage: ChatMessage = {
        id: crypto.randomUUID(),
        text,
        sender: "user",
        timestamp: new Date(),
      };

      setMessages((prev) => [...prev, userMessage]);
      setIsLoading(true);

      const assistantMessageId = crypto.randomUUID();
      let accumulatedText = "";

      try {
        await startStream(
          {
            userMessage: text,
            threadId: threadIdRef.current,
            chatType: "Global",
            userId: user.userId,
            pageContext: {
              jsonContext: JSON.stringify(pageContext),
            },
            userLanguage: i18n.language,
          },
          (delta: string) => {
            accumulatedText += delta;
            setMessages((prev) => {
              const existingIndex = prev.findIndex(
                (m) => m.id === assistantMessageId,
              );
              const assistantMessage: ChatMessage = {
                id: assistantMessageId,
                text: accumulatedText,
                sender: "assistant",
                timestamp: new Date(),
              };

              if (existingIndex >= 0) {
                const updated = [...prev];
                updated[existingIndex] = assistantMessage;
                return updated;
              }
              return [...prev, assistantMessage];
            });
          },
          () => {
            setIsLoading(false);
          },
        );
      } catch (error) {
        console.error("Error sending message:", error);
        setIsLoading(false);

        const errorMessage: ChatMessage = {
          id: crypto.randomUUID(),
          text: "Sorry, I encountered an error. Please try again.",
          sender: "assistant",
          timestamp: new Date(),
        };
        setMessages((prev) => [...prev, errorMessage]);
      }
    },
    [pageContext, user?.userId, i18n.language, startStream],
  );

  return {
    messages,
    sendMessage,
    isLoading,
  };
};
