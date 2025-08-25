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
};

export type SignalRNotificationType = "Success" | "Info" | "Warning" | "Error";

export type SignalRNotificationMessage = {
  message: string;
  type: SignalRNotificationType;
  timestamp: string;
};
