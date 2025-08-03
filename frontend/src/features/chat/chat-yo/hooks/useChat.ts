import { useState } from "react";
import { askAzureOpenAI } from "../services";

export interface ChatMessage {
  position: "left" | "right";
  type: "text";
  sender: "user" | "system";
  text: string;
  date: Date;
}

export const useChat = () => {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [loading, setLoading] = useState(false);

  const sendMessage = async (text: string) => {
    if (!text.trim()) return;

    // Add user message immediately
    const userMsg: ChatMessage = {
      position: "right",
      type: "text",
      sender: "user",
      text,
      date: new Date(),
    };
    setMessages((prev) => [...prev, userMsg]);

    setLoading(true);

    // Ask Azure OpenAI
    const aiResponse = await askAzureOpenAI(text);

    const aiMsg: ChatMessage = {
      position: "left",
      type: "text",
      sender: "system",
      text: aiResponse,
      date: new Date(),
    };

    setMessages((prev) => [...prev, aiMsg]);
    setLoading(false);
  }

  return { messages, loading, sendMessage };
};
