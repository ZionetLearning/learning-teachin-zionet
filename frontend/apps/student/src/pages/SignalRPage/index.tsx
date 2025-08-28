import { useCallback, useState } from "react";
import { Box, Paper, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { useSignalR, useSignalREvent } from "@student/hooks";
import { usePostTask } from "@student/api";
import { TaskForm, TaskInput, NotificationFeed } from "./components";
import { SignalRNotificationMessage } from "@/types";
import { useStyles } from "./style";

export const SignalRPage = () => {
  const { t } = useTranslation();
  const classes = useStyles();
  const { status, userId } = useSignalR();
  const { mutate: postTask, isPending } = usePostTask();

  const [notifications, setNotifications] = useState<
    SignalRNotificationMessage[]
  >([]);

  const handleNotificationMessage = useCallback(
    (n: SignalRNotificationMessage) => {
      setNotifications((prev) => [...prev, n]);
    },
    [],
  );

  useSignalREvent<[SignalRNotificationMessage]>(
    "NotificationMessage",
    handleNotificationMessage,
  );

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
