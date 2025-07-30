import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App.tsx";
import { AppInsightsErrorBoundary } from "./components";
import { appInsights } from "./appInsights";
import { ReactQueryProvider } from "./providers";

appInsights.loadAppInsights();

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <AppInsightsErrorBoundary boundaryName="FrontendRootApp">
      <ReactQueryProvider>
        <App />
      </ReactQueryProvider>
    </AppInsightsErrorBoundary>
  </StrictMode>,
);
