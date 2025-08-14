import { StrictMode } from "react";
import "./i18n";
import { createRoot } from "react-dom/client";
import * as Sentry from "@sentry/react";
import { initializeSentry } from "./sentry";
import {
  ReactQueryProvider,
  I18nTranslateProvider,
  AuthProvider,
} from "./providers";
import "./index.css";
import App from "./App.tsx";

initializeSentry();

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <I18nTranslateProvider>
      <ReactQueryProvider>
        <AuthProvider>
          <Sentry.ErrorBoundary fallback={<div>Something went wrong</div>}>
            <App />
          </Sentry.ErrorBoundary>{" "}
        </AuthProvider>
      </ReactQueryProvider>
    </I18nTranslateProvider>
  </StrictMode>
);
