import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { ErrorMessage, Field, Form, Formik, FormikHelpers } from "formik";
import { useNavigate } from "react-router-dom";

import { AppRoleType, useAuth } from "@app-providers";
import { useStyles } from "./style";
import {
  loginSchema,
  LoginValues,
  signupSchema,
  SignupValues,
} from "./validation";

const authMode = {
  login: "login",
  signup: "signup",
} as const;

type AuthModeType = (typeof authMode)[keyof typeof authMode];

interface AuthorizationPageProps {
  allowedRoles: AppRoleType[];
}

export const AuthorizationPage = ({ allowedRoles }: AuthorizationPageProps) => {
  const classes = useStyles();
  const { login, logout, signup, role, isAuthorized, loginStatus, appRole } =
    useAuth();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [mode, setMode] = useState<AuthModeType>(authMode.login);
  const [authError, setAuthError] = useState<string | null>(null);

  const extractErrorMessage = (err: unknown): string => {
    if (!err) return t("pages.auth.genericError");
    if (err instanceof Error)
      return err.message || t("pages.auth.genericError");
    if (typeof err === "string") return err;
    if (typeof err === "object") {
      const rec = err as Record<string, unknown>;
      const msg = rec.message;
      if (typeof msg === "string" && msg.trim()) return msg;
    }
    return t("pages.auth.genericError");
  };

  useEffect(function applyFullScreen() {
    document.body.classList.add("auth-fullscreen");
    return () => {
      document.body.classList.remove("auth-fullscreen");
    };
  }, []);

  useEffect(
    function redirectAfterLogin() {
      if (!isAuthorized) return;
      if (!role) return;
      if (!allowedRoles.includes(role)) {
        setAuthError(t("pages.auth.forbiddenRole"));
        logout();
        return;
      }
      const to = sessionStorage.getItem("redirectAfterLogin") || "/";
      sessionStorage.removeItem("redirectAfterLogin");
      navigate(to, { replace: true });
    },
    [allowedRoles, isAuthorized, navigate, role, t, logout],
  );

  useEffect(
    function resetError() {
      setAuthError(null);
    },
    [mode],
  );

  return (
    <main className={classes.authPageBackground} data-testid="auth-page">
      <header className={classes.authPageHeader}>
        <h1 className={classes.authPageTitle}>
          {t("pages.auth.welcomeToLearningTeachinZionet")}
        </h1>
      </header>
      <div className={classes.authPageContent}>
        <div className={classes.authPageContainer}>
          <div
            className={classes.authPageSubtitle}
            data-testid="auth-role-heading"
          >
            {appRole === "teacher"
              ? t("pages.auth.teacherDashboard")
              : appRole === "student"
                ? t("pages.auth.studentDashboard")
                : t("pages.auth.adminDashboard")}
          </div>
          <div className={classes.authPageTabs}>
            {[authMode.login, authMode.signup].map((tab) => (
              <button
                key={tab}
                onClick={() => setMode(tab as AuthModeType)}
                className={`${classes.authPageTab} ${mode === tab ? "active" : ""}`}
                data-testid={`auth-tab-${tab}`}
              >
                {tab === authMode.login
                  ? t("pages.auth.login")
                  : t("pages.auth.signup")}
              </button>
            ))}
          </div>
          {authError && (
            <div className={classes.authPageError} data-testid="auth-error">
              {authError}
            </div>
          )}
          {mode === authMode.login ? (
            <Formik<LoginValues>
              key="login"
              initialValues={{ email: "", password: "" }}
              validationSchema={loginSchema}
              onSubmit={async (
                values,
                { setSubmitting }: FormikHelpers<LoginValues>,
              ) => {
                try {
                  await login(values.email, values.password);
                } catch (e) {
                  setAuthError(extractErrorMessage(e));
                } finally {
                  setSubmitting(false);
                }
              }}
            >
              {({ isSubmitting }) => (
                <Form className={classes.authPageForm}>
                  <div>
                    <Field
                      name="email"
                      type="email"
                      placeholder={t("pages.auth.email")}
                      className={classes.authPageInput}
                      autoComplete="email"
                      data-testid="auth-email"
                    />
                    <div className={classes.authPageError}>
                      <ErrorMessage name="email" />
                    </div>
                  </div>
                  <div>
                    <Field
                      name="password"
                      type="password"
                      placeholder={t("pages.auth.password")}
                      className={classes.authPageInput}
                      autoComplete="current-password"
                      data-testid="auth-password"
                    />
                    <div className={classes.authPageError}>
                      <ErrorMessage name="password" />
                    </div>
                  </div>
                  <button
                    type="submit"
                    disabled={isSubmitting || loginStatus?.isLoading}
                    className={classes.authPageSubmit}
                    data-testid="auth-submit"
                  >
                    {isSubmitting || loginStatus?.isLoading
                      ? t("pages.auth.loggingIn")
                      : t("pages.auth.login")}
                  </button>
                </Form>
              )}
            </Formik>
          ) : (
            <Formik<SignupValues>
              key="signup"
              initialValues={{
                firstName: "",
                lastName: "",
                email: "",
                password: "",
                confirmPassword: "",
              }}
              validationSchema={signupSchema}
              onSubmit={async (
                values,
                { setSubmitting }: FormikHelpers<SignupValues>,
              ) => {
                try {
                  await signup({
                    email: values.email,
                    password: values.password,
                    firstName: values.firstName,
                    lastName: values.lastName,
                    role: appRole,
                  });
                } catch (e) {
                  setAuthError(extractErrorMessage(e));
                } finally {
                  setSubmitting(false);
                }
              }}
            >
              {({ isSubmitting }) => (
                <Form className={classes.authPageForm}>
                  {authError && (
                    <div
                      className={classes.authPageError}
                      data-testid="auth-error"
                    >
                      {authError}
                    </div>
                  )}
                  <div>
                    <Field
                      name="firstName"
                      placeholder={t("pages.auth.firstName")}
                      className={classes.authPageInput}
                      autoComplete="given-name"
                    />
                    <div className={classes.authPageError}>
                      <ErrorMessage name="firstName" />
                    </div>
                  </div>
                  <div>
                    <Field
                      name="lastName"
                      placeholder={t("pages.auth.lastName")}
                      className={classes.authPageInput}
                      autoComplete="family-name"
                    />
                    <div className={classes.authPageError}>
                      <ErrorMessage name="lastName" />
                    </div>
                  </div>
                  <div>
                    <Field
                      name="email"
                      type="email"
                      placeholder={t("pages.auth.email")}
                      className={classes.authPageInput}
                      autoComplete="email"
                      data-testid="auth-email"
                    />
                    <div className={classes.authPageError}>
                      <ErrorMessage name="email" />
                    </div>
                  </div>
                  <div>
                    <Field
                      name="password"
                      type="password"
                      placeholder={t("pages.auth.password")}
                      className={classes.authPageInput}
                      autoComplete="new-password"
                      data-testid="auth-password"
                    />
                    <div className={classes.authPageError}>
                      <ErrorMessage name="password" />
                    </div>
                  </div>
                  <div>
                    <Field
                      name="confirmPassword"
                      type="password"
                      placeholder={t("pages.auth.confirmPassword")}
                      className={classes.authPageInput}
                      autoComplete="new-password"
                      data-testid="auth-confirm-password"
                    />
                    <div className={classes.authPageError}>
                      <ErrorMessage name="confirmPassword" />
                    </div>
                  </div>
                  <button
                    type="submit"
                    disabled={isSubmitting || loginStatus?.isLoading}
                    className={classes.authPageSubmit}
                    data-testid="auth-submit"
                  >
                    {isSubmitting || loginStatus?.isLoading
                      ? t("pages.auth.creatingAccount")
                      : t("pages.auth.signup")}
                  </button>
                </Form>
              )}
            </Formik>
          )}
        </div>
      </div>
    </main>
  );
};
