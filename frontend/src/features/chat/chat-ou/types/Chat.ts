import type { Message, MessageContext } from "./Message";

export interface ChatState {
  messages: Message[];
  isLoading: boolean;
  currentContext?: MessageContext;
  error?: string;
}

export interface MessageStore {
  state: ChatState;
  actions: {
    addMessage: (message: Message) => void;
    setLoading: (loading: boolean) => void;
    setContext: (context: MessageContext) => void;
    clearMessages: () => void;
  };
}

export interface ChatError {
  type: "network" | "validation" | "rendering" | "context";
  message: string;
  details?: unknown;
  timestamp: Date;
}

export interface ErrorHandler {
  handleError(error: ChatError): void;
  displayError(error: ChatError): void;
  retryAction(action: () => Promise<void>): Promise<void>;
}
