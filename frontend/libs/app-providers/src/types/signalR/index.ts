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

export type StreamStage = "First" | "Chunk" | "Last" | "Heartbeat" | "Error";

export type StreamMessage<T = unknown> = {
  payload: T;
  sequenceNumber: number;
  stage: StreamStage;
};

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
  waitForStream: <T = unknown>(
    eventType: string,
    requestId: string,
    onMessage?: (msg: StreamMessage<T>) => void,
    timeoutMs?: number,
  ) => Promise<T[]>;
  subscribeToStream: <T = unknown>(
    eventType: string,
    requestId: string,
    handler: (msg: StreamMessage<T>) => void,
  ) => () => void;
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

export interface StreamEvent<TPayload = unknown> {
  eventType: string; // StreamEventType from backend (e.g., "ChatAiAnswer")
  payload: TPayload;
  sequenceNumber: number;
  stage: "First" | "Chunk" | "Last" | "Heartbeat" | "Error";
  requestId: string;
}

export interface SystemMessagePayload {
  code: number;
  description: string;
}

// Regular sentence types
export interface SentenceItem {
  exerciseId: string;
  text: string;
  words: string[];
  difficulty: string;
  nikud: boolean;
}

// Split sentence types
export interface SplitSentenceItem {
  exerciseId: string;
  words: string[];
  text: string;
  difficulty: string;
  nikud: boolean;
}


export interface SentenceGenerationResponse {
  requestId: string;
  sentences: SentenceItem[];
}

export interface SplitSentenceGenerationResponse {
  requestId: string;
  sentences: SplitSentenceItem[];
}

// Union Type for all possible events
export type UserEventUnion =
  | { eventType: typeof EventType.ChatAiAnswer; payload: ChatAiAnswerPayload }
  | { eventType: typeof EventType.SystemMessage; payload: SystemMessagePayload }
  | {
      eventType: typeof EventType.SentenceGeneration;
      payload: SentenceGenerationResponse;
    }
  | {
      eventType: typeof EventType.SplitSentenceGeneration;
      payload: SplitSentenceGenerationResponse;
    };

// Event Handler Type
export type EventHandler<T = unknown> = (event: T) => void;
