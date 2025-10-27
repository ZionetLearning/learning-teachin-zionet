import { useEffect, useMemo, useState } from "react";

import ClearIcon from "@mui/icons-material/Clear";
import SearchIcon from "@mui/icons-material/Search";
import {
  IconButton,
  InputAdornment,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TablePagination,
  TableRow,
  TextField,
} from "@mui/material";
import { useTranslation } from "react-i18next";

import { useGetAllUsers } from "@admin/api";
import { UserListItem } from "..";
import { useStyles } from "./style";

export const UsersTable = ({ dir }: { dir: "ltr" | "rtl" }) => {
  const { t } = useTranslation();
  const classes = useStyles();

  const {
    data: users,
    isLoading: isUsersLoading,
    error: getUsersError,
  } = useGetAllUsers();

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);

  useEffect(() => setPage(0), [search]);

  const filteredUsers = useMemo(() => {
    if (!users) return [];
    const q = search.trim().toLowerCase();
    if (!q) return users;
    return users.filter((u) =>
      [u.email, u.firstName, u.lastName, u.role].some((field) =>
        field.toLowerCase().includes(q),
      ),
    );
  }, [users, search]);

  const currentPageUsers = useMemo(() => {
    if (rowsPerPage <= 0) return filteredUsers;
    const start = page * rowsPerPage;
    return filteredUsers.slice(start, start + rowsPerPage);
  }, [filteredUsers, page, rowsPerPage]);

  const handlePageChange = (_: unknown, newPage: number) => setPage(newPage);

  const handleRowsPerPageChange = (
    event: React.ChangeEvent<HTMLInputElement>,
  ) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  return (
    <div className={classes.listContainer} data-testid="users-list">
      <h2 className={classes.sectionTitle}>{t("pages.users.users")}</h2>

      {isUsersLoading && <p>{t("pages.users.loadingUsers")}</p>}
      {getUsersError && (
        <p style={{ color: "#c00" }}>{t("pages.users.userNotFound")}</p>
      )}

      {!isUsersLoading && !getUsersError && (
        <div className={classes.tableArea} data-testid="users-table">
          <div className={classes.tableShell} data-testid="users-table-shell">
            <div className={classes.tableScrollX}>
              <Table
                size="small"
                className={classes.headerTable}
                aria-label="users header"
              >
                <TableHead>
                  <TableRow>
                    <TableCell align="center" width="28%">
                      {t("pages.users.email")}
                    </TableCell>
                    <TableCell align="center" width="18%">
                      {t("pages.users.firstName")}
                    </TableCell>
                    <TableCell align="center" width="18%">
                      {t("pages.users.lastName")}
                    </TableCell>
                    <TableCell align="center" width="16%">
                      {t("pages.users.role")}
                    </TableCell>
                    <TableCell align="center" width="20%">
                      {t("pages.users.actions")}
                    </TableCell>
                  </TableRow>
                </TableHead>
              </Table>
            </div>

            <div className={classes.searchBar} data-testid="users-search-bar">
              <TextField
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder={t("pages.users.searchPlaceholder")}
                size="small"
                fullWidth
                slotProps={{
                  htmlInput: { "data-testid": "users-search-input" },
                  input: {
                    startAdornment: (
                      <InputAdornment position="start">
                        <SearchIcon fontSize="small" />
                      </InputAdornment>
                    ),
                    endAdornment: search ? (
                      <InputAdornment position="end">
                        <IconButton
                          size="small"
                          aria-label={t("pages.users.clear")}
                          onClick={() => setSearch("")}
                          data-testid="users-search-clear"
                        >
                          <ClearIcon fontSize="small" />
                        </IconButton>
                      </InputAdornment>
                    ) : null,
                  },
                }}
                className={classes.searchField}
                dir={dir}
              />
            </div>

            <div className={classes.rowsScroll} data-testid="users-rows-scroll">
              <div className={classes.tableScrollX}>
                <Table
                  size="small"
                  className={classes.bodyTable}
                  aria-label="users body"
                >
                  <TableBody>
                    {currentPageUsers.map((u) => (
                      <UserListItem key={u.userId} user={u} />
                    ))}
                    {filteredUsers.length === 0 && (
                      <TableRow>
                        <TableCell colSpan={5}>
                          {t("pages.users.noUsersFound")}
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </div>
            </div>
          </div>

          <TablePagination
            component="div"
            className={classes.paginationBar}
            data-testid="users-pagination"
            count={filteredUsers.length}
            page={page}
            onPageChange={handlePageChange}
            rowsPerPage={rowsPerPage}
            onRowsPerPageChange={handleRowsPerPageChange}
            rowsPerPageOptions={[
              5,
              10,
              25,
              { label: t("pages.users.all"), value: -1 },
            ]}
            labelRowsPerPage={t("pages.users.rowsPerPage")}
            labelDisplayedRows={({ from, to, count }) =>
              `${from}-${to} ${t("pages.users.of")} ${count !== -1 ? count : to}`
            }
          />
        </div>
      )}
    </div>
  );
};
