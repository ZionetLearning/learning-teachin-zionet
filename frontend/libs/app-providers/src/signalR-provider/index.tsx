import { useEffect, useState, useMemo, useRef, useCallback } from "react";
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { SignalRContext } from "@app-providers/context";
import { useAuth } from "@app-providers/auth";
import { decodeJwtPayload } from "@app-providers/auth/utils";
import type {
  SignalRContextType,
  SignalRProviderProps,
  EventType,
  UserEventUnion,
  UserNotification,
  Status,
  StreamEvent,
  StreamMessage,
} from "@app-providers/types";

type ActiveStream<T = unknown> = {
  eventType: string;
  requestId: string;
  messages: StreamMessage<T>[];
  subscribers: Set<(msg: StreamMessage<T>) => void>;
  resolve: (msgs: T[]) => void;
  reject: (reason?: unknown) => void;
  timeout: NodeJS.Timeout;
};

export const SignalRProvider = ({ hubUrl, children }: SignalRProviderProps) => {
  const [status, setStatus] = useState<Status>("idle");
  const { accessToken } = useAuth();
  const connRef = useRef<HubConnection | null>(null);

  const userId = useMemo(() => {
    if (!accessToken) return null;
    const payload = decodeJwtPayload(accessToken);
    if (!payload) return null;

    const extractedUserId = payload.userId as string;

    return extractedUserId;
  }, [accessToken]);

  // Store event handlers for different event types
  const handlersRef = useRef<Map<string, Set<(data: unknown) => void>>>(
    new Map(),
  );
  // Store active streams
  const activeStreamsRef = useRef<Map<string, ActiveStream>>(new Map());

  // Store pending requests for waitForResponse
  const pendingRequestsRef = useRef<
    Map<
      string,
      {
        resolve: (value: unknown) => void;
        reject: (reason?: unknown) => void;
        timeout: NodeJS.Timeout;
      }
    >
  >(new Map());

  const handleEvent = useCallback((eventName: string, data: unknown) => {
    const handlers = handlersRef.current.get(eventName);
    if (handlers) {
      handlers.forEach((handler) => handler(data));
    }
  }, []);

  const handleStreamEvent = useCallback((evt: StreamEvent<unknown>) => {
    const { eventType, requestId, payload, sequenceNumber, stage } = evt;
    const streamKey = `${eventType}:${requestId}`;

    const activeStream = activeStreamsRef.current.get(streamKey);
    if (!activeStream) return;

    const msg: StreamMessage = { payload, sequenceNumber, stage };
    activeStream.messages.push(msg);

    // Notify subscribers immediately
    activeStream.subscribers.forEach((cb) => cb(msg));

    if (stage === "Last" || stage === "Error") {
      clearTimeout(activeStream.timeout);
      activeStreamsRef.current.delete(streamKey);

      if (stage === "Error") {
        activeStream.reject(new Error(`Stream ${streamKey} ended with error`));
      } else {
        const ordered = activeStream.messages.sort(
          (a, b) => a.sequenceNumber - b.sequenceNumber,
        );
        activeStream.resolve(ordered.map((m) => m.payload));
      }
    }
  }, []);

  const checkPendingRequests = useCallback(
    (eventType: string, payload: unknown) => {
      if (payload && typeof payload === "object" && "requestId" in payload) {
        const requestId = (payload as { requestId: string }).requestId;
        const requestKey = `${eventType}:${requestId}`;

        const pendingRequest = pendingRequestsRef.current.get(requestKey);
        if (pendingRequest) {
          clearTimeout(pendingRequest.timeout);
          pendingRequestsRef.current.delete(requestKey);
          pendingRequest.resolve(payload);
        }
      } else if (
        payload &&
        typeof payload === "object" &&
        "RequestId" in payload
      ) {
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
    },
    [],
  );

  const subscribe = useCallback(
    <T = unknown,>(
      eventName: string,
      handler: (data: T) => void,
    ): (() => void) => {
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
    },
    [],
  );

  const subscribeToStream = useCallback(
    <T = unknown,>(
      eventType: string,
      requestId: string,
      handler: (msg: StreamMessage<T>) => void,
    ): (() => void) => {
      const streamKey = `${eventType}:${requestId}`;
      const activeStream = activeStreamsRef.current.get(streamKey);
      if (!activeStream) {
        throw new Error(`No active stream for ${streamKey}`);
      }

      activeStream.subscribers.add(handler as (msg: StreamMessage) => void);
      return () => {
        activeStream.subscribers.delete(
          handler as (msg: StreamMessage) => void,
        );
      };
    },
    [],
  );

  const waitForResponse = useCallback(
    <T = unknown,>(
      eventType: EventType,
      requestId: string,
      timeoutMs: number = 300000,
    ): Promise<T> => {
      if (status !== "connected") {
        return Promise.reject(new Error("SignalR is not connected"));
      }

      return new Promise<T>((resolve, reject) => {
        const requestKey = `${eventType}:${requestId}`;

        // Set up timeout
        const timeout = setTimeout(() => {
          pendingRequestsRef.current.delete(requestKey);
          reject(
            new Error(
              `Timeout waiting for ${eventType} with requestId ${requestId}`,
            ),
          );
        }, timeoutMs);

        pendingRequestsRef.current.set(requestKey, {
          resolve: resolve as (value: unknown) => void,
          reject,
          timeout,
        });
      });
    },
    [status],
  );

  const waitForStream = useCallback(
    <T = unknown,>(
      eventType: string,
      requestId: string,
      onMessage?: (msg: StreamMessage<T>) => void,
      timeoutMs = 300000,
    ): Promise<T[]> => {
      if (status !== "connected") {
        return Promise.reject(new Error("SignalR is not connected"));
      }

      const streamKey = `${eventType}:${requestId}`;
      if (activeStreamsRef.current.has(streamKey)) {
        return Promise.reject(
          new Error(`Stream ${streamKey} already in progress`),
        );
      }

      return new Promise<T[]>((resolve, reject) => {
        const timeout = setTimeout(() => {
          activeStreamsRef.current.delete(streamKey);
          reject(new Error(`Timeout waiting for stream ${streamKey}`));
        }, timeoutMs);

        const newStream: ActiveStream<T> = {
          eventType,
          requestId,
          messages: [],
          subscribers: new Set(onMessage ? [onMessage] : []),
          resolve,
          reject,
          timeout,
        };

        activeStreamsRef.current.set(
          streamKey,
          newStream as ActiveStream<unknown>,
        );
      });
    },
    [status],
  );

  useEffect(() => {
    if (!accessToken || !userId) {
      setStatus("idle");
      return;
    }

    let isMounted = true;
    const pendingRequests = pendingRequestsRef.current;

    const connection = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => {
          return accessToken || "";
        },
        withCredentials: true,
      })
      .withAutomaticReconnect()
      .build();

    connection.onreconnecting(() => {
      if (isMounted) setStatus("reconnecting");
    });
    connection.onreconnected(() => {
      if (isMounted) setStatus("connected");
    });
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
        eventTypeHandlers.forEach((handler) => handler(event.payload));
      }

      // Check for pending requests
      checkPendingRequests(event.eventType, event.payload);
    });

    // Listen for stream events (from SendStreamEventAsync)
    connection.on("StreamEvent", (streamEvent: StreamEvent<unknown>) => {
      handleStreamEvent(streamEvent);
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
  }, [
    hubUrl,
    accessToken,
    userId,
    handleEvent,
    checkPendingRequests,
    handleStreamEvent,
  ]);

  useEffect(function cleanUp() {
    const handleBeforeUnload = () => {
      const c = connRef.current;
      if (c) {
        c.stop().catch(() => {});
      }
    };

    const handlePageHide = () => {
      const c = connRef.current;
      if (c) {
        c.stop().catch(() => {});
      }
    };

    window.addEventListener("beforeunload", handleBeforeUnload);
    window.addEventListener("pagehide", handlePageHide);
    return () => {
      window.removeEventListener("beforeunload", handleBeforeUnload);
      window.removeEventListener("pagehide", handlePageHide);
    };
  }, []);

  const value = useMemo<SignalRContextType>(
    () => ({
      connection: connRef.current,
      status,
      userId: userId ?? "",
      subscribe,
      waitForResponse,
      waitForStream,
      subscribeToStream,
    }),
    [
      status,
      userId,
      subscribe,
      waitForResponse,
      waitForStream,
      subscribeToStream,
    ],
  );

  // Expose status to window for E2E tests (non-production side effect, harmless in prod)
  useEffect(() => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (window as any).__signalRStatus = status;
  }, [status]);

  return (
    <SignalRContext.Provider value={value}>{children}</SignalRContext.Provider>
  );
};
