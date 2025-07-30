import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App.tsx";
import { AppInsightsErrorBoundary } from "./components";
import { appInsights } from "./appInsights";
import { I18nTranslateProvider } from "./providers/i18n-translate-provider";

appInsights.loadAppInsights();

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <I18nTranslateProvider>
      <AppInsightsErrorBoundary boundaryName="FrontendRootApp">
        <App />
      </AppInsightsErrorBoundary>
    </I18nTranslateProvider>
  </StrictMode>,
);
