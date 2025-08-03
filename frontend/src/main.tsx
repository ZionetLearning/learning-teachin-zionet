import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { ReactQueryProvider, I18nTranslateProvider } from "./providers";
import "./index.css";
import App from "./App.tsx";
import { AppInsightsErrorBoundary } from "./components";
import { appInsights } from "./appInsights";

appInsights.loadAppInsights();

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <I18nTranslateProvider>
      <ReactQueryProvider>
        <AppInsightsErrorBoundary boundaryName="FrontendRootApp">
          <App />
        </AppInsightsErrorBoundary>
      </ReactQueryProvider>
    </I18nTranslateProvider>
  </StrictMode>
);
