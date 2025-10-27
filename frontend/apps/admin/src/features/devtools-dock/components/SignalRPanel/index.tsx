import { useEffect, useState } from "react";
import { useSignalR } from "@admin/hooks";
import { useStyles } from "./style";

export const SignalRPanel = () => {
  const classes = useStyles();
  const { status, subscribe } = useSignalR();

  const [events, setEvents] = useState<string[]>([]);
  const [reconnects, setReconnects] = useState(0);
  const [errors, setErrors] = useState(0);

  useEffect(() => {
    if (!subscribe) return;

    // list of SignalR events we care about
    const eventsToWatch = [
      "connected",
      "reconnecting",
      "reconnected",
      "closed",
      "error",
    ];

    const unsubscribers = eventsToWatch.map((eventName) =>
      subscribe(eventName, (payload: unknown) => {
        const ts = new Date().toISOString();
        setEvents((prev) =>
          [
            `[${ts}] ${eventName}${payload ? ` ${JSON.stringify(payload)}` : ""}`,
            ...prev,
          ].slice(0, 100),
        );

        if (eventName === "reconnecting" || eventName === "reconnected") {
          setReconnects((c) => c + 1);
        }
        if (eventName === "error") {
          setErrors((c) => c + 1);
        }
      }),
    );

    // cleanup on unmount
    return () => {
      unsubscribers.forEach((unsub) => {
        unsub();
      });
    };
  }, [subscribe]);

  const healthClass =
    status === "connected" ? "ok" : status === "reconnecting" ? "warn" : "";

  return (
    <div>
      <div className={classes.grid}>
        <div>Status</div>
        <div className={classes.pill}>
          <span className={`${classes.dot} ${healthClass}`} />
          <span>{status}</span>
        </div>

        <div>Reconnects</div>
        <div>{reconnects}</div>

        <div>Errors</div>
        <div>{errors}</div>
      </div>

      <div className={classes.log}>
        {events.length ? events.join("\n") : "No SignalR events yet."}
      </div>
    </div>
  );
};
