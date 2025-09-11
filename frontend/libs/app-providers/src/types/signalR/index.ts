import { ReactNode } from "react";
import { HubConnection } from "@microsoft/signalr";

export type SignalRProviderProps = {
  hubUrl: string;
  children: ReactNode;
};

export type Status =
  | "idle"
  | "connecting"
  | "connected"
  | "reconnecting"
  | "disconnected";

export type SignalRContextType = {
  connection: HubConnection | null;
  status: Status;
  userId: string;
  subscribe: <T = unknown>(
    eventName: string,
    handler: (data: T) => void,
  ) => () => void;
  waitForResponse: <T = unknown>(
    eventType: EventType,
    requestId: string,
    timeoutMs?: number,
  ) => Promise<T>;
};

export type SignalRNotificationType = "Success" | "Info" | "Warning" | "Error";

export interface UserNotification {
  message: string;
  type: SignalRNotificationType;
  timestamp: string;
}

export const EventType = {
  ChatResponse: "ChatResponse",
  ChatAiAnswer: "ChatAiAnswer",
  Notification: "Notification",
  SystemMessage: "SystemMessage",
  SentenceGeneration: "SentenceGeneration", // Must match backend exactly
  SplitSentenceGeneration: "SplitSentenceGeneration", // Must match backend exactly
} as const;

export type EventType = (typeof EventType)[keyof typeof EventType];

export interface ChatAiAnswerPayload {
  requestId: string;
  assistantMessage?: string;
  chatName: string;
  status: string;
  threadId: string;
}

export interface NotificationPayload {
  message: string;
  severity: "info" | "warning" | "error";
}

export interface SystemMessagePayload {
  code: number;
  description: string;
}

// Regular sentence types
export interface SentenceItem {
  text: string;
  difficulty: string;
  nikud: boolean;
}

export interface SentenceGeneratedPayload {
  sentences: SentenceItem[];
}

// Split sentence types
export interface SplitSentenceItem {
  words: string[];
  original: string;
  difficulty: string;
  nikud: boolean;
}

export interface SplitSentenceGeneratedPayload {
  sentences: SplitSentenceItem[];
}

// Union Type for all possible events
export type UserEventUnion =
  | { eventType: typeof EventType.ChatAiAnswer; payload: ChatAiAnswerPayload }
  | { eventType: typeof EventType.SystemMessage; payload: SystemMessagePayload }
  | {
      eventType: typeof EventType.SentenceGeneration;
      payload: SentenceGeneratedPayload;
    }
  | {
      eventType: typeof EventType.SplitSentenceGeneration;
      payload: SplitSentenceGeneratedPayload;
    };

// Event Handler Type
export type EventHandler<T = unknown> = (event: T) => void;
