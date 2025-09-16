import { createRoot } from "react-dom/client";
import { ToastContainer } from "react-toastify";
import * as Sentry from "@sentry/react";
import {
  ReactQueryProvider,
  I18nTranslateProvider,
  AuthProvider,
  SignalRProvider,
  initAppInsights,
  initializeSentry
} from "@app-providers";
import { AppRole } from "@app-providers/types";
import "./index.css";
import App from "./App.tsx";

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
          <Sentry.ErrorBoundary fallback={<div>Something went wrong</div>}>
          <App />
          <ToastContainer />
          </Sentry.ErrorBoundary>{" "}
        </SignalRProvider>
      </AuthProvider>
    </ReactQueryProvider>
  </I18nTranslateProvider>,
);
