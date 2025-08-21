import { useEffect, useMemo, useRef, useState } from "react";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { Status, SignalRContextType, SignalRProviderProps } from "@/types";
import { SignalRContext } from "@/context";

const getOrCreateUserId = (): string => {
  const key = "sig_user_id";
  let id = localStorage.getItem(key);
  if (!id) {
    id =
      crypto?.randomUUID?.() ??
      `${Date.now()}-${Math.random().toString(16).slice(2)}`;
    localStorage.setItem(key, id);
  }
  return id;
};

export const SignalRProvider = ({ hubUrl, children }: SignalRProviderProps) => {
  const [status, setStatus] = useState<Status>("idle");
  const [userId] = useState(() => getOrCreateUserId());
  const connRef = useRef<HubConnection | null>(null);

  useEffect(() => {
    let isMounted = true;

    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
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
    [status, userId]
  );

  return (
    <SignalRContext.Provider value={value}>{children}</SignalRContext.Provider>
  );
};
