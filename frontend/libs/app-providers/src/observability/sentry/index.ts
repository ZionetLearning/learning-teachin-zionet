import * as Sentry from "@sentry/react";

export interface SentryProps {
  appName: "student" | "teacher" | "admin";
}
export const initializeSentry = ({ appName }: SentryProps) => {
  const allowedOrigins = [
    /https?:\/\/teachin\.westeurope\.cloudapp\.azure\.com/,
    /https?:\/\/[a-z0-9-]+\.1\.azurestaticapps\.net/,
  ];

  Sentry.init({
    dsn: import.meta.env.VITE_SENTRY_DSN,
    sendDefaultPii: false,
    release: import.meta.env.VITE_RELEASE, //tells Sentry to tag all events with that same release name so uploaded sourcemaps match our errors
    allowUrls: allowedOrigins,
    replaysSessionSampleRate: 0, // to not record any normal sessions (without errors)
    replaysOnErrorSampleRate: 1.0, // record all error sessions (1.0 => 100%)
    integrations: [
      Sentry.replayIntegration({
        maskAllText: false,
        blockAllMedia: false,
      }),
      Sentry.thirdPartyErrorFilterIntegration({
        filterKeys: ["teach-in-app"],
        behaviour: "drop-error-if-contains-third-party-frames", //for example chrome extensions
      }),
    ],
  });
  Sentry.setTag("app", appName);
};
export { SentryErrorBoundary } from "./ErrorBoundary";