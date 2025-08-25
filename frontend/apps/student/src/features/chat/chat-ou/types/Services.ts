import type { Message, MessageContext, MessageSender } from "./Message";

export interface MessageService {
  sendMessage(content: string, context?: MessageContext): Promise<void>;
  getMessages(): Message[];
  subscribeToMessages(callback: (messages: Message[]) => void): () => void;
}

export interface ContextService {
  getCurrentPageContext(): MessageContext;
  attachContext(message: string): MessageContext;
  extractSelectedText(): string | undefined;
  getPageMetadata(): Record<string, unknown>;
}

export interface MockAIService {
  generateResponse(
    userMessage: string,
    context?: MessageContext,
  ): Promise<Message>;
  simulateTyping(): Promise<void>;
  simulateNetworkDelay(): Promise<void>;
  getInitialConversation(): MockConversation;
}

export interface MockConversation {
  id: string;
  title: string;
  messages: Message[];
  participants: MessageSender[];
}
