import { useCallback, useState } from "react";
import { Box, Paper, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { useEffect } from "react";
import { useSignalR } from "@/hooks";
import { usePostTask } from "@/api";
import { TaskForm, TaskInput, NotificationFeed } from "./components";
import type { UserNotification } from "@/types/signalR";
import { useStyles } from "./style";

export const SignalRPage = () => {
  const { t } = useTranslation();
  const classes = useStyles();
  const { status, userId, subscribe } = useSignalR();
  const { mutate: postTask, isPending } = usePostTask();

  const [notifications, setNotifications] = useState<UserNotification[]>([]);

  const handleNotificationMessage = useCallback(
    (n: UserNotification) => {
      setNotifications((prev) => [...prev, n]);
    },
    [],
  );

  // subscribe to notification messages (new API)
  useEffect(() => {
  const unsubscribe = subscribe<UserNotification>(
      "NotificationMessage",
      handleNotificationMessage,
    );
    return unsubscribe;
  }, [subscribe, handleNotificationMessage]);

  // submit with API, TaskForm resets itself
  const handleSubmit = useCallback(
    (task: TaskInput, reset: () => void) => {
      postTask(task, { onSuccess: reset });
    },
    [postTask],
  );

  return (
    <Box className={classes.container}>
      <Typography variant="h4" gutterBottom>
        {t("pages.signalR.title")}
      </Typography>
      <Typography variant="h5" gutterBottom>
        {t("pages.signalR.description")}
      </Typography>

      <Typography variant="h6">
        SignalR status:
        <b style={{ color: status === "connected" ? "green" : "red" }}>
          {status}
        </b>
        <br /> userId: {userId}
      </Typography>

      <TaskForm
        isPending={isPending}
        disabled={status !== "connected"}
        onSubmit={handleSubmit}
        defaultId="1"
      />

      <Paper sx={{ mt: 2, p: 2, maxWidth: 520, width: "100%" }}>
        <Typography variant="h6">{t("pages.signalR.notifications")}</Typography>
        <NotificationFeed
          items={notifications}
          emptyText={t("pages.signalR.noNotifications")}
        />
      </Paper>
    </Box>
  );
};
