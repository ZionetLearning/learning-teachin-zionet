import {
  Avatar,
  Box,
  Card,
  CardContent,
  Chip,
  TableCell,
  TableRow,
  Typography,
  useTheme,
} from "@mui/material";
import { useTranslation } from "react-i18next";

import type { OnlineUserDto } from "@app-providers";
import { getRoleColor, getRoleIcon } from "../../utils";
import { useStyles } from "./style";

interface UserRowProps {
  user: OnlineUserDto;
  variant: "table" | "card";
}

export const UserRow = ({ user, variant }: UserRowProps) => {
  const theme = useTheme();
  const classes = useStyles();
  const { t, i18n } = useTranslation();
  const isRTL = i18n.dir() === "rtl";

  if (variant === "table") {
    return (
      <TableRow className={classes.tableRow}>
        <TableCell>
          <Box className={classes.userInfo}>
            <Avatar
              className={classes.avatar}
              style={{ backgroundColor: getRoleColor(user.role, theme) }}
            >
              {getRoleIcon(user.role)}
            </Avatar>
            <Typography className={classes.userName}>{user.name}</Typography>
          </Box>
        </TableCell>
        <TableCell>
          <Chip
            label={user.role || t("pages.users.unknown")}
            size="small"
            className={classes.roleChip}
            style={{ backgroundColor: getRoleColor(user.role, theme) }}
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
        <TableCell align={isRTL ? "left" : "right"}>
          <Typography className={classes.connectionCount}>
            {user.connectionsCount}
          </Typography>
        </TableCell>
      </TableRow>
    );
  }

  return (
    <Card className={classes.mobileCard}>
      <CardContent className={classes.mobileCardContent}>
        <Box className={classes.mobileUserHeader}>
          <Avatar
            className={classes.avatar}
            style={{ backgroundColor: getRoleColor(user.role, theme) }}
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
              style={{ backgroundColor: getRoleColor(user.role, theme) }}
            />
          </Box>
        </Box>
        <Box className={classes.mobileConnectionInfo}>
          <Typography variant="body2" className={classes.connectionCount}>
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
  );
};
