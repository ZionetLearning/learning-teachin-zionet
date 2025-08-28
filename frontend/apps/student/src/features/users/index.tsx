import { ErrorMessage, Field, Form, Formik } from "formik";
import { useTranslation } from "react-i18next";
import { toast } from "react-toastify";

import { useCreateUser, useGetAllUsers } from "@student/api";
import { UserListItem } from "./components";
import { useStyles } from "./style";
import { CreateUserFormValues, validationSchema } from "./validation";

export const Users = () => {
  const { t } = useTranslation();
  const {
    data: users,
    isLoading: isUsersLoading,
    error: getUsersError,
  } = useGetAllUsers();

  const { mutate: createUser, isPending: isCreatingUser } = useCreateUser();

  const initialValues: CreateUserFormValues = { email: "", password: "" };

  const classes = useStyles();

  return (
    <div className={classes.root} data-testid="users-page">
      <div className={classes.formContainer}>
        <h2 className={classes.sectionTitle}>{t("pages.users.createUsers")}</h2>
        <Formik
          initialValues={initialValues}
          validationSchema={validationSchema}
          onSubmit={(values, { resetForm, setSubmitting }) => {
            createUser(
              {
                userId: crypto.randomUUID(),
                email: values.email,
                passwordHash: values.password,
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
        <ul className={classes.list} data-testid="users-list">
          {users?.map((user) => (
            <UserListItem
              key={user.userId}
              userId={user.userId}
              email={user.email}
            />
          ))}
          {!isUsersLoading && users?.length === 0 && !getUsersError && (
            <li>{t("pages.users.noUsersFound")}</li>
          )}
        </ul>
      </div>
    </div>
  );
};
