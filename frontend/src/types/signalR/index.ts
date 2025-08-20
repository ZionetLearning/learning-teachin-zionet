import { HubConnection } from "@microsoft/signalr";

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
