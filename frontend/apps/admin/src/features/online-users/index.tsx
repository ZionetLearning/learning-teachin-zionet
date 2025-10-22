import { useEffect } from "react";
import {
  Box,
  Chip,
  CircularProgress,
  Typography,
  useTheme,
} from "@mui/material";
import { useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";

import { useGetOnlineUsers } from "@admin/api";
import { useSignalR } from "@admin/hooks";
import type { OnlineUserDto } from "@app-providers";
import { OnlineUsersTable, UserRow } from "./components";
import { useStyles } from "./style";

export const OnlineUsers = () => {
  const { data, isLoading, isError } = useGetOnlineUsers();
  const { connection, status } = useSignalR();
  const qc = useQueryClient();
  const theme = useTheme();
  const classes = useStyles({ theme });
  const { t } = useTranslation();

  useEffect(
    function setupSignalRSubscriptionAndEventHandlers() {
      if (!connection || status !== "connected") return;

      connection
        .invoke("SubscribeAdmin")
        .then(() => {})
        .catch((e) => {
          console.error("SubscribeAdmin failed:", e);
          console.log("Connection state:", connection.state);
        });

      const onUserOnline = (userId: string, role: string, name: string) => {
        qc.setQueryData<OnlineUserDto[] | undefined>(
          ["onlineUsers"],
          (prev) => {
            const list = prev ?? [];
            const idx = list.findIndex((u) => u.userId === userId);
            const next: OnlineUserDto = {
              userId,
              name,
              role,
              connectionsCount: idx >= 0 ? list[idx].connectionsCount : 1,
            };
            if (idx >= 0) {
              const copy = list.slice();
              copy[idx] = next;
              return copy;
            }
            return [...list, next];
          },
        );
      };

      const onUserOffline = (userId: string) => {
        qc.setQueryData<OnlineUserDto[] | undefined>(
          ["onlineUsers"],
          (prev) => {
            const list = prev ?? [];
            return list.filter((u) => u.userId !== userId);
          },
        );
      };

      const onUpdateUserConnections = (
        userId: string,
        connectionsCount: number,
      ) => {
        qc.setQueryData<OnlineUserDto[] | undefined>(
          ["onlineUsers"],
          (prev) => {
            const list = prev ?? [];
            const idx = list.findIndex((u) => u.userId === userId);
            if (idx >= 0) {
              const copy = list.slice();
              copy[idx] = { ...copy[idx], connectionsCount };
              return copy;
            }
            return list;
          },
        );
      };

      connection.on("UserOnline", onUserOnline);
      connection.on("UserOffline", onUserOffline);
      connection.on("UpdateUserConnections", onUpdateUserConnections);

      return function cleanupSignalRSubscriptionAndEventHandlers() {
        if (connection && connection.state === "Connected") {
          connection
            .invoke("UnSubscribeAdmin")
            .then(() => {})
            .catch((e) => console.warn("UnSubscribeAdmin failed", e));
        }
        connection?.off("UserOnline", onUserOnline);
        connection?.off("UserOffline", onUserOffline);
        connection?.off("UpdateUserConnections", onUpdateUserConnections);
      };
    },
    [connection, status, qc],
  );

  if (isLoading) {
    return (
      <Box className={classes.loadingContainer}>
        <CircularProgress />
        <Typography variant="body2" color="textSecondary">
          {t("pages.users.loadingOnlineUsers")}
        </Typography>
      </Box>
    );
  }

  if (isError) {
    return (
      <Box className={classes.errorContainer}>
        <Typography variant="h6" color="error">
          {t("pages.users.errorLoadingOnlineUsers")}
        </Typography>
        <Typography variant="body2" color="textSecondary">
          {t("pages.users.errorLoadingOnlineUsersSubtext")}
        </Typography>
      </Box>
    );
  }

  return (
    <Box className={classes.container}>
      <Box className={classes.header}>
        <Typography variant="h4" className={classes.title}>
          {t("pages.users.onlineUsersTitle")} ({data?.length || 0})
        </Typography>
        <Typography variant="body2" className={classes.subtitle}>
          {t("pages.users.onlineUsersSubtitle")}
        </Typography>

        <Chip
          icon={
            <Box
              className={`${classes.statusDot} ${
                status === "connected"
                  ? classes.statusDotConnected
                  : classes.statusDotDisconnected
              }`}
            />
          }
          label={`${t("pages.users.signalRStatus")}: ${status}`}
          variant="outlined"
          size="small"
          className={classes.statusChip}
          color={status === "connected" ? "success" : "warning"}
        />
      </Box>

      <Box className={classes.contentArea}>
        {!data || data.length === 0 ? (
          <Box className={classes.emptyState}>
            <Typography className={classes.emptyIcon}>👥</Typography>
            <Typography variant="h6" gutterBottom>
              {t("pages.users.noUsersOnline")}
            </Typography>
            <Typography variant="body2" color="textSecondary">
              {t("pages.users.noUsersOnlineSubtext")}
            </Typography>
          </Box>
        ) : (
          <>
            <OnlineUsersTable users={data} />
            <Box className={classes.mobileCardList}>
              {data.map((user) => (
                <UserRow key={user.userId} user={user} variant="card" />
              ))}
            </Box>
          </>
        )}
      </Box>
    </Box>
  );
};
