import { createContext } from "react";
import type { HubConnection } from "@microsoft/signalr";

export type SignalRStatus =
  | "idle"
  | "connecting"
  | "connected"
  | "reconnecting"
  | "disconnected";

export type SignalRContextType = {
  connection: HubConnection | null;
  status: SignalRStatus;
  userId: string;
};

export const SignalRContext = createContext<SignalRContextType | null>(null);
