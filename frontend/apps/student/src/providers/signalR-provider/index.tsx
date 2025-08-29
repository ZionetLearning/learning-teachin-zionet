import React, { useEffect, useState, useMemo, useRef, useCallback } from "react";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { SignalRContext } from "@/context";
import type { 
  SignalRContextType, 
  SignalRProviderProps, 
  EventType, 
  UserEventUnion, 
  UserNotification,
  Status
} from "@/types/signalR";

const createUserId = (): string => {
  return crypto?.randomUUID?.() ?? 
    Array.from(new Uint8Array(16), (b) => b.toString(16).padStart(2, "0")).join("");
};

export const SignalRProvider: React.FC<SignalRProviderProps> = ({ 
  hubUrl, 
  children 
}) => {
  const [status, setStatus] = useState<Status>("idle");
  const [userId] = useState(() => createUserId());
  const connRef = useRef<HubConnection | null>(null);
  
  // Store event handlers for different event types
  const handlersRef = useRef<Map<string, Set<(data: unknown) => void>>>(new Map());
  
  // Store pending requests for waitForResponse
  const pendingRequestsRef = useRef<Map<string, {
    resolve: (value: unknown) => void;
    reject: (reason?: unknown) => void;
    timeout: NodeJS.Timeout;
  }>>(new Map());

  const handleEvent = useCallback((eventName: string, data: unknown) => {
    const handlers = handlersRef.current.get(eventName);
    if (handlers) {
      handlers.forEach(handler => handler(data));
    }
  }, []);

  const checkPendingRequests = useCallback((eventType: string, payload: unknown) => {
    if (payload && typeof payload === 'object' && 'requestId' in payload) {
      const requestId = (payload as { requestId: string }).requestId;
      const requestKey = `${eventType}:${requestId}`;
      
      const pendingRequest = pendingRequestsRef.current.get(requestKey);
      if (pendingRequest) {
        clearTimeout(pendingRequest.timeout);
        pendingRequestsRef.current.delete(requestKey);
        pendingRequest.resolve(payload);
      }
    } else if (payload && typeof payload === 'object' && 'RequestId' in payload) {
      // Check for capital R RequestId as well
      const requestId = (payload as { RequestId: string }).RequestId;
      const requestKey = `${eventType}:${requestId}`;
      
      const pendingRequest = pendingRequestsRef.current.get(requestKey);
      if (pendingRequest) {
        clearTimeout(pendingRequest.timeout);
        pendingRequestsRef.current.delete(requestKey);
        pendingRequest.resolve(payload);
      }
    }
  }, []);

  const subscribe = useCallback(<T = unknown>(eventName: string, handler: (data: T) => void): (() => void) => {
    if (!handlersRef.current.has(eventName)) {
      handlersRef.current.set(eventName, new Set());
    }
    
    const handlers = handlersRef.current.get(eventName)!;
    handlers.add(handler as (data: unknown) => void);

    // Return unsubscribe function
    return () => {
      handlers.delete(handler as (data: unknown) => void);
      if (handlers.size === 0) {
        handlersRef.current.delete(eventName);
      }
    };
  }, []);

  const waitForResponse = useCallback(<T = unknown>(
    eventType: keyof typeof EventType, 
    requestId: string, 
    timeoutMs: number = 300000
  ): Promise<T> => {
    if (status !== "connected") {
      return Promise.reject(new Error("SignalR is not connected"));
    }

    return new Promise<T>((resolve, reject) => {
      const requestKey = `${eventType}:${requestId}`;
      
      // Set up timeout
      const timeout = setTimeout(() => {
        pendingRequestsRef.current.delete(requestKey);
        reject(new Error(`Timeout waiting for ${eventType} with requestId ${requestId}`));
      }, timeoutMs);

      pendingRequestsRef.current.set(requestKey, {
        resolve: resolve as (value: unknown) => void,
        reject,
        timeout,
      });
    });
  }, [status]);

  useEffect(() => {
    let isMounted = true;
    const pendingRequests = pendingRequestsRef.current;

    const connection = new HubConnectionBuilder()
      .withUrl(`${hubUrl}?userId=${userId}`, {
        withCredentials: false,
      })
      .withAutomaticReconnect()
      .build();

    connection.onreconnecting(() => isMounted && setStatus("reconnecting"));
    connection.onreconnected(() => isMounted && setStatus("connected"));
    connection.onclose(() => {
      if (isMounted) setStatus("disconnected");
      
      // Clear all pending requests
      pendingRequests.forEach(({ reject, timeout }) => {
        clearTimeout(timeout);
        reject(new Error("Connection closed"));
      });
      pendingRequests.clear();
    });

    // Listen for general events (from SendEventAsync)
     connection.on("ReceiveEvent", (event: UserEventUnion) => {
      handleEvent("ReceiveEvent", event);
      
      // Also trigger specific event type handlers
      const eventTypeHandlers = handlersRef.current.get(event.eventType);
      if (eventTypeHandlers) {
        eventTypeHandlers.forEach(handler => handler(event.payload));
      }

      // Check for pending requests
      checkPendingRequests(event.eventType, event.payload);
    });

    // Listen for direct notifications (from SendNotificationAsync)
    connection.on("NotificationMessage", (notification: UserNotification) => {
      handleEvent("NotificationMessage", notification);
    });

    connRef.current = connection;

    (async () => {
      try {
        setStatus("connecting");
        await connection.start();
        if (!isMounted) return;
        setStatus("connected");
      } catch (e) {
        console.error("SignalR connection failed:", e);
        if (isMounted) setStatus("disconnected");
      }
    })();

    return () => {
      isMounted = false;
      
      // Clear all pending requests
      pendingRequests.forEach(({ reject, timeout }) => {
        clearTimeout(timeout);
        reject(new Error("Connection closed"));
      });
      pendingRequests.clear();
      
      const c = connRef.current;
      connRef.current = null;
      c?.stop().catch(() => {});
    };
  }, [hubUrl, userId, handleEvent, checkPendingRequests]);

  const value = useMemo<SignalRContextType>(
    () => ({ 
      connection: connRef.current, 
      status, 
      userId,
      subscribe,
      waitForResponse,
    }),
    [status, userId, subscribe, waitForResponse],
  );

  return (
    <SignalRContext.Provider value={value}>
      {children}
    </SignalRContext.Provider>
  );
};
