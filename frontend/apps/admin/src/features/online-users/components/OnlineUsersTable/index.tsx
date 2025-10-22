import {
  Box,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  useTheme,
} from "@mui/material";
import { useTranslation } from "react-i18next";

import type { OnlineUserDto } from "@app-providers";
import { UserRow } from "../UserRow";
import { useStyles } from "./style";

interface OnlineUsersTableProps {
  users: OnlineUserDto[];
}

export const OnlineUsersTable = ({ users }: OnlineUsersTableProps) => {
  const theme = useTheme();
  const classes = useStyles({ theme });
  const { t } = useTranslation();

  return (
    <Box className={classes.desktopTable}>
      <TableContainer component={Paper} className={classes.tableContainer}>
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
            {users.map((user) => (
              <UserRow key={user.userId} user={user} variant="table" />
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
};
