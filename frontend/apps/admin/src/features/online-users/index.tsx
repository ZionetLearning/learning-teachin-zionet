import { Box, CircularProgress, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";

import { useGetOnlineUsers } from "@admin/api";
import { useOnlineUsersSync } from "@admin/hooks";
import { OnlineUsersTable, UserRow } from "./components";
import { useStyles } from "./style";

export const OnlineUsers = () => {
  const { data, isLoading, isError } = useGetOnlineUsers();
  const classes = useStyles();
  const { t } = useTranslation();

  useOnlineUsersSync();

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
