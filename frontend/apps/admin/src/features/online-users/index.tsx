import { useEffect } from "react";
import {
  Box,
  Typography,
  Chip,
  CircularProgress,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Avatar,
  Card,
  CardContent,
  useTheme,
} from "@mui/material";
import type { Theme } from "@mui/material/styles";
import { useTranslation } from "react-i18next";
import { useGetOnlineUsers } from "@admin/api";
import { useSignalR } from "@admin/hooks";
import { useQueryClient } from "@tanstack/react-query";
import type { OnlineUserDto } from "@app-providers";
import { useStyles } from "./style";

const getRoleColor = (role: string | null | undefined, theme: Theme) => {
  if (!role) return theme.palette.grey[500];

  switch (role.toLowerCase()) {
    case "teacher":
      return theme.palette.primary.main;
    case "student":
      return theme.palette.success.main;
    case "admin":
      return theme.palette.warning.main;
    default:
      return theme.palette.grey[500];
  }
};

const getRoleIcon = (role: string | null | undefined) => {
  if (!role) return "👤";

  switch (role.toLowerCase()) {
    case "teacher":
      return "👨‍🏫";
    case "student":
      return "👨‍🎓";
    case "admin":
      return "👑";
    default:
      return "👤";
  }
};

export const OnlineUsers = () => {
  const { data, isLoading, isError } = useGetOnlineUsers();
  const { connection, status } = useSignalR();
  const qc = useQueryClient();
  const theme = useTheme();
  const classes = useStyles({ theme });
  const { t } = useTranslation();

  useEffect(() => {
    if (!connection || status !== "connected") {
      console.log("SignalR not ready:", { connection: !!connection, status });
      return;
    }

    console.log("Attempting to subscribe admin...");

    connection
      .invoke("SubscribeAdmin")
      .then(() => {
        console.log("SubscribeAdmin successful");
      })
      .catch((e) => {
        console.error("SubscribeAdmin failed:", e);
        console.log("Connection state:", connection.state);
      });

    const onUserOnline = (userId: string, role: string, name: string) => {
      qc.setQueryData<OnlineUserDto[] | undefined>(["onlineUsers"], (prev) => {
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
      });
    };

    const onUserOffline = (userId: string) => {
      qc.setQueryData<OnlineUserDto[] | undefined>(["onlineUsers"], (prev) => {
        const list = prev ?? [];
        return list.filter((u) => u.userId !== userId);
      });
    };

    connection.on("UserOnline", onUserOnline);
    connection.on("UserOffline", onUserOffline);

    return () => {
      if (connection && connection.state === "Connected") {
        connection
          .invoke("UnSubscribeAdmin")
          .then(() => console.log("UnSubscribeAdmin successful"))
          .catch((e) => console.warn("UnSubscribeAdmin failed", e));
      }
      connection?.off("UserOnline", onUserOnline);
      connection?.off("UserOffline", onUserOffline);
    };
  }, [connection, status, qc]);

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
        <Typography
          variant="body2"
          color="textSecondary"
          className={classes.subtitle}
        >
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
          <Box className={classes.desktopTable}>
            <TableContainer
              component={Paper}
              className={classes.tableContainer}
            >
              <Table>
                <TableHead className={classes.tableHead}>
                  <TableRow>
                    <TableCell>{t("pages.users.user")}</TableCell>
                    <TableCell>{t("pages.users.role")}</TableCell>
                    <TableCell>{t("pages.users.status")}</TableCell>
                    <TableCell align="right">
                      {t("pages.users.connections")}
                    </TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {data.map((user) => (
                    <TableRow key={user.userId} className={classes.tableRow}>
                      <TableCell>
                        <Box className={classes.userInfo}>
                          <Avatar
                            className={classes.avatar}
                            style={{
                              backgroundColor: getRoleColor(user.role, theme),
                            }}
                          >
                            {getRoleIcon(user.role)}
                          </Avatar>
                          <Typography className={classes.userName}>
                            {user.name}
                          </Typography>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={user.role || t("pages.users.unknown")}
                          size="small"
                          className={classes.roleChip}
                          style={{
                            backgroundColor: getRoleColor(user.role, theme),
                          }}
                        />
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={t("pages.users.online")}
                          size="small"
                          color="success"
                          variant="outlined"
                          className={classes.onlineChip}
                        />
                      </TableCell>
                      <TableCell align="right">
                        <Typography className={classes.connectionCount}>
                          {user.connectionsCount}
                        </Typography>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </Box>

          <Box className={classes.mobileCardList}>
            {data.map((user) => (
              <Card key={user.userId} className={classes.mobileCard}>
                <CardContent className={classes.mobileCardContent}>
                  <Box className={classes.mobileUserHeader}>
                    <Avatar
                      className={classes.avatar}
                      style={{
                        backgroundColor: getRoleColor(user.role, theme),
                      }}
                    >
                      {getRoleIcon(user.role)}
                    </Avatar>
                    <Box className={classes.mobileUserInfo}>
                      <Typography className={classes.mobileUserName}>
                        {user.name}
                      </Typography>
                      <Chip
                        label={user.role || t("pages.users.unknown")}
                        size="small"
                        className={classes.roleChip}
                        style={{
                          backgroundColor: getRoleColor(user.role, theme),
                        }}
                      />
                    </Box>
                  </Box>
                  <Box className={classes.mobileConnectionInfo}>
                    <Typography
                      variant="body2"
                      className={classes.connectionCount}
                    >
                      {user.connectionsCount}{" "}
                      {user.connectionsCount === 1
                        ? t("pages.users.connection")
                        : t("pages.users.connections")}
                    </Typography>
                    <Chip
                      label={t("pages.users.online")}
                      size="small"
                      color="success"
                      variant="outlined"
                      className={classes.onlineChip}
                    />
                  </Box>
                </CardContent>
              </Card>
            ))}
          </Box>
        </>
      )}
    </Box>
  );
};
