import { createRoot } from "react-dom/client";
import { ToastContainer } from "react-toastify";
import * as Sentry from "@sentry/react";
import { initializeSentry } from "@app-providers/observability";
import {
  ReactQueryProvider,
  I18nTranslateProvider,
  AuthProvider,
  initAppInsights,
} from "@app-providers";
import { AppRole } from "@app-providers/types";
import "./index.css";
import App from "./App.tsx";

initAppInsights("admin");
initializeSentry({ appName: "admin" });

createRoot(document.getElementById("root")!).render(
  <I18nTranslateProvider>
    <ReactQueryProvider>
      <AuthProvider appRole={AppRole.admin}>
        <Sentry.ErrorBoundary fallback={<div>Something went wrong</div>}>
        <App />
        <ToastContainer />
        </Sentry.ErrorBoundary>{" "}
      </AuthProvider>
    </ReactQueryProvider>
  </I18nTranslateProvider>,
);
