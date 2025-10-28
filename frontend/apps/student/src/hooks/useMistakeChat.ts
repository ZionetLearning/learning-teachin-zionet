import { useState, useCallback } from "react";
import {
  useSendChatMessageStream,
  useSendMistakeExplanation,
  ChatType,
} from "@student/api";
import { useAuth } from "@app-providers/auth";

export interface MistakeChatMessage {
  text: string;
  sender: "user" | "bot";
  isTyping?: boolean;
}

export interface UseMistakeChatOptions {
  attemptId: string;
  threadId: string;
  gameType: string;
}

export const useMistakeChat = ({
  attemptId,
  threadId,
  gameType,
}: UseMistakeChatOptions) => {
  const { user } = useAuth();
  const { startMistakeExplanationStream } = useSendMistakeExplanation();
  const { startStream } = useSendChatMessageStream();

  const [messages, setMessages] = useState<MistakeChatMessage[]>([]);
  const [isStreaming, setIsStreaming] = useState(false);
  const [hasInitialized, setHasInitialized] = useState(false);

  const initialize = useCallback(async () => {
    if (!user?.userId || hasInitialized || isStreaming) {
      return;
    }

    setIsStreaming(true);
    setHasInitialized(true);

    const typingMessage: MistakeChatMessage = {
      text: "",
      sender: "bot",
      isTyping: true,
    };

    setMessages([typingMessage]);

    let assistantBuffer = "";

    try {
      await startMistakeExplanationStream(
        {
          attemptId,
          threadId,
          gameType,
          chatType: ChatType.ExplainMistake,
        },
        (delta: string) => {
          assistantBuffer += delta;

          setMessages((prev) => {
            const updated = [...prev];
            const lastIndex = updated.length - 1;
            const lastMessage = updated[lastIndex];

            if (lastMessage?.sender === "bot" && lastMessage.isTyping) {
              updated[lastIndex] = {
                ...lastMessage,
                text: assistantBuffer,
                isTyping: true,
              };
            }

            return updated;
          });
        },
        () => {
          setMessages((prev) => {
            const updated = [...prev];
            const lastIndex = updated.length - 1;
            const lastMessage = updated[lastIndex];

            if (lastMessage?.sender === "bot" && lastMessage.isTyping) {
              updated[lastIndex] = {
                ...lastMessage,
                text: assistantBuffer,
                isTyping: false,
              };
            }

            return updated;
          });
          setIsStreaming(false);
        }
      );
    } catch (error) {
      console.error("Failed to request mistake explanation:", error);
      setMessages([
        {
          text: "Sorry, I couldn't load the explanation. Please try again.",
          sender: "bot",
          isTyping: false,
        },
      ]);
      setIsStreaming(false);
    }
  }, [
    user?.userId,
    hasInitialized,
    isStreaming,
    startMistakeExplanationStream,
    attemptId,
    threadId,
    gameType,
  ]);

  const sendMessage = useCallback(
    async (messageText: string) => {
      if (!user?.userId || !messageText.trim() || isStreaming) return;

      const userMessage: MistakeChatMessage = {
        text: messageText.trim(),
        sender: "user",
        isTyping: false,
      };

      setMessages((prev) => [...prev, userMessage]);
      setIsStreaming(true);

      const typingMessage: MistakeChatMessage = {
        text: "",
        sender: "bot",
        isTyping: true,
      };

      setMessages((prev) => [...prev, typingMessage]);

      let assistantBuffer = "";

      try {
        await startStream(
          {
            userMessage: messageText.trim(),
            threadId,
            chatType: ChatType.ExplainMistake,
            userId: user.userId,
          },
          (delta: string) => {
            assistantBuffer += delta;

            setMessages((prev) => {
              const updated = [...prev];
              const lastIndex = updated.length - 1;
              const lastMessage = updated[lastIndex];

              if (lastMessage?.sender === "bot" && lastMessage.isTyping) {
                updated[lastIndex] = {
                  ...lastMessage,
                  text: assistantBuffer,
                  isTyping: true,
                };
              }

              return updated;
            });
          },
          () => {
            setMessages((prev) => {
              const updated = [...prev];
              const lastIndex = updated.length - 1;
              const lastMessage = updated[lastIndex];

              if (lastMessage?.sender === "bot" && lastMessage.isTyping) {
                updated[lastIndex] = {
                  ...lastMessage,
                  text: assistantBuffer,
                  isTyping: false,
                };
              }

              return updated;
            });
            setIsStreaming(false);
          }
        );
      } catch (error) {
        console.error("Failed to send message:", error);
        setIsStreaming(false);
      }
    },
    [user?.userId, threadId, startStream, isStreaming]
  );

  const reset = useCallback(() => {
    setMessages([]);
    setHasInitialized(false);
    setIsStreaming(false);
  }, []);

  return {
    messages,
    sendMessage,
    initialize,
    reset,
    loading: isStreaming,
    hasInitialized,
  };
};
