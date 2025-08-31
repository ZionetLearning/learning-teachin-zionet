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
  subscribe: <T = unknown>(eventName: string, handler: (data: T) => void) => () => void;
  waitForResponse: <T = unknown>(eventType: keyof typeof EventType, requestId: string, timeoutMs?: number) => Promise<T>;
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
} as const;

export type EventType = typeof EventType[keyof typeof EventType];

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

// Union Type for all possible events, here we add new models we want to get by signalR
export type UserEventUnion =
  | { eventType: typeof EventType.ChatAiAnswer; payload: ChatAiAnswerPayload }
  | { eventType: typeof EventType.SystemMessage; payload: SystemMessagePayload };

// Event Handler Type
export type EventHandler<T = unknown> = (event: T) => void;