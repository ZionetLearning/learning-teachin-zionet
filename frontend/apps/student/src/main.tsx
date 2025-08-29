import "./i18n";
import { createRoot } from "react-dom/client";
import { ToastContainer } from "react-toastify";
import * as Sentry from "@sentry/react";
import { initializeSentry } from "./sentry";
import {
  ReactQueryProvider,
  I18nTranslateProvider,
  AuthProvider,
  SignalRProvider,
} from "./providers";
import "./index.css";
import App from "./App.tsx";

initializeSentry();

// const HUB_URL = "http://localhost:5280/notificationHub";
const HUB_URL = import.meta.env.VITE_BASE_URL!;

createRoot(document.getElementById("root")!).render(
  <I18nTranslateProvider>
    <ReactQueryProvider>
      <AuthProvider>
        <SignalRProvider hubUrl={`${HUB_URL}/notificationHub`}>
          <Sentry.ErrorBoundary fallback={<div>Something went wrong</div>}>
            <App />
            <ToastContainer />
          </Sentry.ErrorBoundary>{" "}
        </SignalRProvider>
      </AuthProvider>
    </ReactQueryProvider>
  </I18nTranslateProvider>,
);

