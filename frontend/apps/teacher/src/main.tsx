import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import * as Sentry from "@sentry/react";
import {
  AppRole,
  AuthProvider,
  initAppInsights,
  I18nTranslateProvider,
  initializeSentry,
  ReactQueryProvider,
} from "@app-providers";
import App from "./App.tsx";

initAppInsights("teacher");
initializeSentry({ appName: "teacher" });

createRoot(document.getElementById("root")!).render(
  <I18nTranslateProvider>
    <ReactQueryProvider>
      <AuthProvider appRole={AppRole.teacher}>
        <StrictMode>
          <Sentry.ErrorBoundary fallback={<div>Something went wrong</div>}>
            <App />
          </Sentry.ErrorBoundary>
        </StrictMode>
      </AuthProvider>,
    </ReactQueryProvider>
  </I18nTranslateProvider>
);
