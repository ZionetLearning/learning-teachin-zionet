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
  SentryErrorBoundary,
  SignalRProvider,
} from "@app-providers";
import App from "./App.tsx";

initAppInsights("teacher");
initializeSentry({ appName: "teacher" });

const BASE_URL = import.meta.env.VITE_BASE_URL!;
const HUB_URL = `${BASE_URL}/notificationHub`;

createRoot(document.getElementById("root")!).render(
  <I18nTranslateProvider>
    <ReactQueryProvider>
      <AuthProvider appRole={AppRole.teacher}>
        <SignalRProvider hubUrl={HUB_URL}>
          <StrictMode>
            <SentryErrorBoundary>
              <App />
            </SentryErrorBoundary>
          </StrictMode>
        </SignalRProvider>
      </AuthProvider>
    </ReactQueryProvider>
  </I18nTranslateProvider>,
);
