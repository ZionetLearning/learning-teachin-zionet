import { useEffect, useState } from "react";

import { ErrorMessage, Field, Form, Formik, FormikHelpers } from "formik";
import { useNavigate } from "react-router-dom";

import { useAuth } from "@/providers/auth";
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

export const AuthorizationPage = () => {
  const classes = useStyles();
  const { login } = useAuth();
  const navigate = useNavigate();
  const [mode, setMode] = useState<AuthModeType>(authMode.login);

  useEffect(function applyFullScreen() {
    document.body.classList.add("auth-fullscreen");
    return () => {
      document.body.classList.remove("auth-fullscreen");
    };
  }, []);

  const handleAuthSuccess = () => {
    const to = sessionStorage.getItem("redirectAfterLogin") || "/";
    sessionStorage.removeItem("redirectAfterLogin");
    navigate(to, { replace: true });
  };

  return (
    <main className={classes.authPageBackground}>
      <header className={classes.authPageHeader}>
        <h1 className={classes.authPageTitle}>
          Welcome to Learning-Teachin-Zionet
        </h1>
      </header>
      <div className={classes.authPageContent}>
        <div className={classes.authPageContainer}>
          <div className={classes.authPageTabs}>
            {[authMode.login, authMode.signup].map((tab) => (
              <button
                key={tab}
                onClick={() => setMode(tab as AuthModeType)}
                className={`${classes.authPageTab} ${mode === tab ? "active" : ""}`}
              >
                {tab === authMode.login ? "Log In" : "Sign Up"}
              </button>
            ))}
          </div>

          {mode === authMode.login ? (
            <Formik<LoginValues>
              key="login"
              initialValues={{ email: "", password: "" }}
              validationSchema={loginSchema}
              onSubmit={(
                values,
                { setSubmitting }: FormikHelpers<LoginValues>,
              ) => {
                login(values.email, values.password);
                setSubmitting(false);
                handleAuthSuccess();
              }}
            >
              {({ isSubmitting }) => (
                <Form className={classes.authPageForm}>
                  <div>
                    <Field
                      name="email"
                      type="email"
                      placeholder="Email"
                      className={classes.authPageInput}
                      autoComplete="email"
                    />
                    <div className={classes.authPageError}>
                      <ErrorMessage name="email" />
                    </div>
                  </div>
                  <div>
                    <Field
                      name="password"
                      type="password"
                      placeholder="Password"
                      className={classes.authPageInput}
                      autoComplete="current-password"
                    />
                    <div className={classes.authPageError}>
                      <ErrorMessage name="password" />
                    </div>
                  </div>
                  <button
                    type="submit"
                    disabled={isSubmitting}
                    className={classes.authPageSubmit}
                  >
                    {isSubmitting ? "Logging in…" : "Log In"}
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
              onSubmit={(
                values,
                { setSubmitting }: FormikHelpers<SignupValues>,
              ) => {
                login(values.email, values.password);
                setSubmitting(false);
                handleAuthSuccess();
              }}
            >
              {({ isSubmitting }) => (
                <Form className={classes.authPageForm}>
                  <div>
                    <Field
                      name="firstName"
                      placeholder="First Name"
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
                      placeholder="Last Name"
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
                      placeholder="Email"
                      className={classes.authPageInput}
                      autoComplete="email"
                    />
                    <div className={classes.authPageError}>
                      <ErrorMessage name="email" />
                    </div>
                  </div>
                  <div>
                    <Field
                      name="password"
                      type="password"
                      placeholder="Password"
                      className={classes.authPageInput}
                      autoComplete="new-password"
                    />
                    <div className={classes.authPageError}>
                      <ErrorMessage name="password" />
                    </div>
                  </div>
                  <div>
                    <Field
                      name="confirmPassword"
                      type="password"
                      placeholder="Confirm Password"
                      className={classes.authPageInput}
                      autoComplete="new-password"
                    />
                    <div className={classes.authPageError}>
                      <ErrorMessage name="confirmPassword" />
                    </div>
                  </div>
                  <button
                    type="submit"
                    disabled={isSubmitting}
                    className={classes.authPageSubmit}
                  >
                    {isSubmitting ? "Creating account…" : "Sign Up"}
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
