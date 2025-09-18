import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import {
  AppRole,
  AuthProvider,
  initAppInsights,
  I18nTranslateProvider,
  initializeSentry,
  ReactQueryProvider,
  SentryErrorBoundary
} from "@app-providers";
import App from "./App.tsx";

initAppInsights("teacher");
initializeSentry({ appName: "teacher" });

createRoot(document.getElementById("root")!).render(
  <I18nTranslateProvider>
    <ReactQueryProvider>
      <AuthProvider appRole={AppRole.teacher}>
        <StrictMode>
          <SentryErrorBoundary>
            <App />
          </SentryErrorBoundary>
        </StrictMode>
      </AuthProvider>
    </ReactQueryProvider>
  </I18nTranslateProvider>,
);
