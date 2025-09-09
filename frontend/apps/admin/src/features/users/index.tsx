import {
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
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

  const initialValues: CreateUserFormValues = {
    email: "",
    password: "",
    firstName: "",
    lastName: "",
    role: AppRole.student,
  };

  const roleOptions = (Object.values(AppRole) as AppRoleType[]).map((r) => ({
    label: t(`roles.${r}`),
    value: r,
  }));

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
      <div className={classes.listContainer} data-testid="users-list-container">
        <h2 className={classes.sectionTitle}>{t("pages.users.users")}</h2>
        {isUsersLoading && <p>{t("pages.users.loadingUsers")}</p>}
        {getUsersError && (
          <p style={{ color: "#c00" }}>{t("pages.users.userNotFound")}</p>
        )}
        {!isUsersLoading && !getUsersError && (
          <TableContainer component={Paper} data-testid="users-table">
            <Table size="small" stickyHeader>
              <TableHead>
                <TableRow>
                  <TableCell align="center">{t("pages.users.email")}</TableCell>
                  <TableCell align="center">
                    {t("pages.users.firstName")}
                  </TableCell>
                  <TableCell align="center">
                    {t("pages.users.lastName")}
                  </TableCell>
                  <TableCell align="center">{t("pages.users.role")}</TableCell>
                  <TableCell align="center">
                    {t("pages.users.actions")}
                  </TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {users?.map((u) => (
                  <UserListItem key={u.userId} user={u} />
                ))}
                {(!users || users.length === 0) && (
                  <TableRow>
                    <TableCell colSpan={5}>
                      {t("pages.users.noUsersFound")}
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </div>
    </div>
  );
};
