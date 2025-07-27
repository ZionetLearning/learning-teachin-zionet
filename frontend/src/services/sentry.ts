import * as Sentry from "@sentry/react";

export const initializeSentry = () => {
  Sentry.init({
    dsn: import.meta.env.VITE_SENTRY_DSN,
    sendDefaultPii: false,
    integrations: [],
  });
};
