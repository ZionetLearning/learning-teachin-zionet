import { useState, useCallback } from "react";
import type { ChatMessage, PageContext } from "./types";

const HARDCODED_RESPONSES = [
  "That's a great question! Based on your current exercise, I can help you understand this better.",
  "Let me explain this in the context of what you're practicing right now.",
  "I see you're working on this exercise. Here's what you need to know...",
  "Good question! This relates directly to the difficulty level you've chosen.",
  "Based on your progress so far, here's my suggestion...",
  "That's an interesting point about this exercise type!",
];

export const useContextAwareChat = (pageContext: PageContext) => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  const sendMessage = useCallback(
    (text: string) => {
      const userMessage: ChatMessage = {
        id: crypto.randomUUID(),
        text,
        sender: "user",
        timestamp: new Date(),
      };

      setMessages((prev) => [...prev, userMessage]);
      setIsLoading(true);

      // Simulate API call with hardcoded response
      setTimeout(() => {
        const randomResponse =
          HARDCODED_RESPONSES[
            Math.floor(Math.random() * HARDCODED_RESPONSES.length)
          ];

        const contextInfo = `\n\n[Context: ${pageContext.pageName}${
          pageContext.exerciseType ? ` - ${pageContext.exerciseType}` : ""
        }${
          pageContext.currentExercise !== undefined
            ? ` - Exercise ${pageContext.currentExercise}/${pageContext.totalExercises}`
            : ""
        }${pageContext.difficulty ? ` - ${pageContext.difficulty}` : ""}]`;

        const assistantMessage: ChatMessage = {
          id: crypto.randomUUID(),
          text: randomResponse + contextInfo,
          sender: "assistant",
          timestamp: new Date(),
        };

        setMessages((prev) => [...prev, assistantMessage]);
        setIsLoading(false);
      }, 1000);
    },
    [pageContext],
  );

  return {
    messages,
    sendMessage,
    isLoading,
  };
};
