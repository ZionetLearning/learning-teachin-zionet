import { useContext, useEffect } from "react";
import { SignalRContext } from "@student/context";

type Handler<TArgs extends unknown[]> = (...args: TArgs) => void;

// Access to the SignalR connection, status and userId.
export const useSignalR = () => {
  const ctx = useContext(SignalRContext);
  if (!ctx) throw new Error("useSignalR must be used within <SignalRProvider>");
  return ctx;
};

// Subscribe to server event with automatic cleanup
export const useSignalREvent = <TArgs extends unknown[] = unknown[]>(
  eventName: string,
  handler: Handler<TArgs>,
) => {
  const { connection } = useSignalR();

  useEffect(() => {
    if (!connection) return;

    connection.on(eventName, handler);
    return () => {
      connection.off(eventName, handler);
    };
  }, [connection, eventName, handler]);
};
