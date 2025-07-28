import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App.tsx";
import { AppInsightsErrorBoundary } from "./components";
import { appInsights } from "./appInsights";

appInsights.loadAppInsights();

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <AppInsightsErrorBoundary boundaryName="FrontendRootApp">
      <App />
    </AppInsightsErrorBoundary>
  </StrictMode>
);
