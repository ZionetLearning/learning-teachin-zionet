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
  TableRow,
  TextField,
  TablePagination,
} from "@mui/material";
import { ErrorMessage, Field, FieldProps, Form, Formik } from "formik";
import { useTranslation } from "react-i18next";
import { toast } from "react-toastify";

import { useGetAllUsers } from "@admin/api";
import { useCreateUser } from "@app-providers";
import { AppRole, AppRoleType } from "@app-providers/types";
import { Dropdown } from "@ui-components";
import { UserListItem } from "./components";
import { useStyles } from "./style";
import { CreateUserFormValues, validationSchema } from "./validation";

export const Users = () => {
  const { t, i18n } = useTranslation();
  const classes = useStyles();

  const dir = i18n.dir();
  const isRtl = dir === "rtl";

  const {
    data: users,
    isLoading: isUsersLoading,
    error: getUsersError,
  } = useGetAllUsers();

  const { mutate: createUser, isPending: isCreatingUser } = useCreateUser();

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  // Sticky logic removed: layout now separates header/search from scrollable rows.

  useEffect(
    function resetOnSearch() {
      setPage(0);
    },
    [search],
  );

  const initialValues: CreateUserFormValues = {
    email: "",
    password: "",
    firstName: "",
    lastName: "",
    role: AppRole.student,
  };

  const roleOptions = useMemo(() => {
    return (Object.values(AppRole) as AppRoleType[]).map((r) => ({
      label: t(`roles.${r}`),
      value: r,
    }));
  }, [t]);

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

  const handlePageChange = (_event: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleRowsPerPageChange = (
    event: React.ChangeEvent<HTMLInputElement>,
  ) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  return (
    <div className={classes.root} data-testid="users-page">
      <div
        className={classes.formContainer}
        style={{ textAlign: isRtl ? "right" : "left" }}
      >
        <h2 className={classes.sectionTitle}>{t("pages.users.createUsers")}</h2>
        <Formik
          initialValues={initialValues}
          validationSchema={validationSchema}
          onSubmit={(values, { resetForm, setSubmitting }) => {
            createUser(
              {
                userId: crypto.randomUUID(),
                email: values.email,
                password: values.password,
                firstName: values.firstName,
                lastName: values.lastName,
                role: values.role,
              },
              {
                onSuccess: () => {
                  toast.success(t("pages.users.userCreated"));
                  resetForm();
                },
                onError: (e) => {
                  toast.error(e.message || t("pages.users.userCreationFailed"));
                },
                onSettled: () => setSubmitting(false),
              },
            );
          }}
        >
          {({ isSubmitting }) => (
            <Form className={classes.form}>
              <label className={classes.label}>
                {t("pages.users.email")}
                <Field
                  name="email"
                  type="email"
                  placeholder="user@example.com"
                  autoComplete="email"
                  data-testid="users-create-email"
                />
                <span className={classes.error}>
                  <ErrorMessage name="email" />
                </span>
              </label>
              <label className={classes.label}>
                {t("pages.users.firstName")}
                <Field
                  name="firstName"
                  type="text"
                  placeholder="John"
                  autoComplete="given-name"
                  data-testid="users-create-first-name"
                />
                <span className={classes.error}>
                  <ErrorMessage name="firstName" />
                </span>
              </label>
              <label className={classes.label}>
                {t("pages.users.lastName")}
                <Field
                  name="lastName"
                  type="text"
                  placeholder="Doe"
                  autoComplete="family-name"
                  data-testid="users-create-last-name"
                />
                <span className={classes.error}>
                  <ErrorMessage name="lastName" />
                </span>
              </label>
              <label className={classes.label}>
                {t("pages.users.role")}
                <Field name="role">
                  {({ field, form, meta }: FieldProps<string>) => (
                    <Dropdown
                      name="role"
                      options={roleOptions}
                      value={field.value}
                      onChange={(val) => form.setFieldValue(field.name, val)}
                      error={meta.touched && !!meta.error}
                      helperText={meta.touched ? meta.error : ""}
                      data-testid="users-create-role"
                    />
                  )}
                </Field>
              </label>
              <label className={classes.label}>
                {t("pages.users.password")}
                <Field
                  name="password"
                  type="password"
                  placeholder="******"
                  autoComplete="new-password"
                  data-testid="users-create-password"
                />
                <span className={classes.error}>
                  <ErrorMessage name="password" />
                </span>
              </label>
              <button
                className={classes.submitButton}
                type="submit"
                disabled={isSubmitting || isCreatingUser}
                data-testid="users-create-submit"
              >
                {isSubmitting || isCreatingUser
                  ? t("pages.users.creating")
                  : t("pages.users.createUser")}
              </button>
            </Form>
          )}
        </Formik>
      </div>
      <div className={classes.listContainer} data-testid="users-list">
        <h2 className={classes.sectionTitle}>{t("pages.users.users")}</h2>
        {isUsersLoading && <p>{t("pages.users.loadingUsers")}</p>}
        {getUsersError && (
          <p style={{ color: "#c00" }}>{t("pages.users.userNotFound")}</p>
        )}
        {!isUsersLoading && !getUsersError && (
          <div className={classes.tableArea} data-testid="users-table">
            <div className={classes.tableShell} data-testid="users-table-shell">
              <Table
                size="small"
                className={classes.headerTable}
                aria-label="users header"
              >
                <TableHead>
                  <TableRow>
                    <TableCell align="center">
                      {t("pages.users.email")}
                    </TableCell>
                    <TableCell align="center">
                      {t("pages.users.firstName")}
                    </TableCell>
                    <TableCell align="center">
                      {t("pages.users.lastName")}
                    </TableCell>
                    <TableCell align="center">
                      {t("pages.users.role")}
                    </TableCell>
                    <TableCell align="center">
                      {t("pages.users.actions")}
                    </TableCell>
                  </TableRow>
                </TableHead>
              </Table>
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
              <div
                className={classes.rowsScroll}
                data-testid="users-rows-scroll"
              >
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
                `${from}-${to} ${t("pages.users.of")} ${
                  count !== -1 ? count : to
                }`
              }
            />
          </div>
        )}
      </div>
    </div>
  );
};
