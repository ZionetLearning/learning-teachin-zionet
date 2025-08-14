import * as Sentry from "@sentry/react";

export const initializeSentry = () => {
  Sentry.init({
    dsn: import.meta.env.VITE_SENTRY_DSN,
    sendDefaultPii: false,
    integrations: [],
    release: import.meta.env.VITE_RELEASE, //tells Sentry to tag all events with that same release name so uploaded sourcemaps match our errors
    allowUrls: [
      /https?:\/\/teachin\.westeurope\.cloudapp\.azure\.com/,
      /https?:\/\/[a-z0-9-]+\.1\.azurestaticapps\.net/,
    ],
  });
};
