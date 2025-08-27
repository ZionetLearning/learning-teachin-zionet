import { useEffect, useMemo, useRef, useState } from "react";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { SignalRContextType, SignalRProviderProps, Status } from "../types";
import { SignalRContext } from "../context";

const createUserId = (): string => {
  const id =
    crypto?.randomUUID?.() ??
    (() => {
      // Secure fallback: generate 16 random bytes and encode as hex
      const arr = new Uint8Array(16);
      window.crypto.getRandomValues(arr);
      return Array.from(arr, (b) => b.toString(16).padStart(2, "0")).join("");
    })();

  return id;
};

export const SignalRProvider = ({ hubUrl, children }: SignalRProviderProps) => {
  const [status, setStatus] = useState<Status>("idle");
  const [userId] = useState(() => createUserId());
  const connRef = useRef<HubConnection | null>(null);

  useEffect(() => {
    let isMounted = true;

    const connection = new HubConnectionBuilder()
      .withUrl(`${hubUrl}?userId=${userId}`, {
        withCredentials: false,
      })
      .withAutomaticReconnect()
      .build();

    connection.onreconnecting(() => isMounted && setStatus("reconnecting"));
    connection.onreconnected(() => isMounted && setStatus("connected"));
    connection.onclose(() => isMounted && setStatus("disconnected"));

    connRef.current = connection;

    (async () => {
      try {
        setStatus("connecting");
        await connection.start();
        if (!isMounted) return;
        setStatus("connected");
      } catch (e) {
        console.error("SignalR start failed", e);
        if (isMounted) setStatus("disconnected");
      }
    })();

    return () => {
      isMounted = false;
      const c = connRef.current;
      connRef.current = null;
      c?.stop().catch(() => {});
    };
  }, [hubUrl, userId]);

  const value = useMemo<SignalRContextType>(
    () => ({ connection: connRef.current, status, userId }),
    [status, userId],
  );

  return (
    <SignalRContext.Provider value={value}>{children}</SignalRContext.Provider>
  );
};
