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
