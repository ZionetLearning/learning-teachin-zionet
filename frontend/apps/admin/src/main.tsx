import { createRoot } from "react-dom/client";
import { ToastContainer } from "react-toastify";
import {
  ReactQueryProvider,
  I18nTranslateProvider,
  AuthProvider,
  SignalRProvider,
  initAppInsights,
  initializeSentry,
  SentryErrorBoundary,
  AppThemeProvider,
} from "@app-providers";
import { DevToolsProvider } from "./providers";
import { AppRole } from "@app-providers/types";
import "./index.css";
import App from "./App.tsx";
import { DevToolsDock } from "./features";

initAppInsights("admin");
initializeSentry({ appName: "admin" });

// SignalR Hub URL
const BASE_URL = import.meta.env.VITE_BASE_URL!;
const HUB_URL = `${BASE_URL}/notificationHub`;

createRoot(document.getElementById("root")!).render(
  <I18nTranslateProvider>
    <ReactQueryProvider>
      <AuthProvider appRole={AppRole.admin}>
        <SignalRProvider hubUrl={HUB_URL}>
          <DevToolsProvider>
            <SentryErrorBoundary>
              <AppThemeProvider>
                <App />
              </AppThemeProvider>
              <DevToolsDock />
              <ToastContainer />
            </SentryErrorBoundary>
          </DevToolsProvider>
        </SignalRProvider>
      </AuthProvider>
    </ReactQueryProvider>
  </I18nTranslateProvider>,
);
