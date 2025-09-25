export type SendMessageRequest = {
  userMessage: string;
  threadId?: string;
  chatType?: "default" | string;
  userId: string; 
};

export type SendMessageResponse = {
  requestId: string;
  assistantMessage?: string;
  chatName: string;
  status: number;
  threadId: string;
};

export type Chat = {
  chatId: string;
  chatName: string;
  chatType: string;
  createdAt: string;
  updatedAt: string;
};

export type ChatMessage = {
  role: string;
  text: string;
  createdAt?: string;
};

export type ChatHistory = {
  chatId: string;
  name: string;
  chatType: string;
  messages: ChatMessage[];
};

export type AIChatStreamResponse = {
  requestId: string;
  threadId: string;
  userId: string;
  chatName: string;
  delta?: string;         // incremental chunk of assistant reply
  sequence: number;       // ordering
  stage: "First" | "Next" | "Last";
  isFinal: boolean;
  elapsedMs: number;
  toolCall?: string;
  toolResult?: string;
};
  