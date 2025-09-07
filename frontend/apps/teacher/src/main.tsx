import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import * as Sentry from "@sentry/react";
import { AuthProvider, initAppInsights, I18nTranslateProvider, initializeSentry} from "@app-providers";
import { AppRole } from "@app-providers/types";
import App from "./App.tsx";

initAppInsights("teacher");
initializeSentry({ appName: "teacher" });

createRoot(document.getElementById("root")!).render(
  <I18nTranslateProvider>
    <AuthProvider appRole={AppRole.teacher}>
      <StrictMode>
        <Sentry.ErrorBoundary fallback={<div>Something went wrong</div>}>
          <App />
        </Sentry.ErrorBoundary>
      </StrictMode>
    </AuthProvider>,
  </I18nTranslateProvider>
);
