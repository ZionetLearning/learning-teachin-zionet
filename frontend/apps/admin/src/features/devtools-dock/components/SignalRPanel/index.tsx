import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { Box } from "@mui/material";
import { useSignalR } from "@admin/hooks";
import { useStyles } from "./style";

export const SignalRPanel = () => {
  const classes = useStyles();
  const { t } = useTranslation();
  const { status, subscribe } = useSignalR();

  const [events, setEvents] = useState<string[]>([]);
  const [reconnects, setReconnects] = useState(0);
  const [errors, setErrors] = useState(0);

  useEffect(() => {
    if (!subscribe) return;

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
    <Box>
      <Box className={classes.grid}>
        <Box>{t("pages.signalR.signalRStatus")}</Box>
        <Box className={classes.pill}>
          <span className={`${classes.dot} ${healthClass}`} />
          <span>{status}</span>
        </Box>

        <Box>{t("pages.signalR.reconnects")}</Box>
        <Box>{reconnects}</Box>

        <Box>{t("pages.signalR.errors")}</Box>
        <Box>{errors}</Box>
      </Box>

      <Box className={classes.log}>
        {events.length ? events.join("\n") : t("pages.signalR.noEventsYet")}
      </Box>
    </Box>
  );
};
