import { useContext, useEffect, useCallback } from "react";
import { SignalRContext } from "@/context";

type Handler<TArgs extends unknown[]> = (...args: TArgs) => void;

/** Access the SignalR connection, status and userId. */
export const useSignalR = () => {
  const ctx = useContext(SignalRContext);
  if (!ctx) throw new Error("useSignalR must be used within <SignalRProvider>");
  return ctx;
};

/** Subscribe to a server event with automatic cleanup. */
export const useSignalREvent = <TArgs extends unknown[] = unknown[]>(
  eventName: string,
  handler: Handler<TArgs>
) => {
  const { connection } = useSignalR();

  useEffect(() => {
    if (!connection) return;

    // Bridge unknown[] from SignalR to your typed handler
    const wrapped = (...args: unknown[]) => {
      handler(...(args as TArgs));
    };

    connection.on(eventName, wrapped);
    return () => {
      connection.off(eventName, wrapped);
    };
  }, [connection, eventName, handler]);
};

/** Invoke a hub method (ensures connection is ready). */
export const useSignalRInvoke = <
  TReq extends unknown[] = unknown[],
  TRes = unknown,
>(
  method: string
) => {
  const { connection } = useSignalR();

  return useCallback(
    async (...args: TReq): Promise<TRes> => {
      if (!connection) throw new Error("SignalR not connected");
      // Cast once at the boundary to satisfy strict typing
      return connection.invoke(
        method,
        ...(args as unknown[])
      ) as unknown as Promise<TRes>;
    },
    [connection, method]
  );
};
