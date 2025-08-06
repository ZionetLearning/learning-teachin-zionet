import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import {
  ReactQueryProvider,
  I18nTranslateProvider,
  AuthProvider,
} from "./providers";
import "./index.css";
import App from "./App.tsx";
import { AppInsightsErrorBoundary } from "./components";
import { appInsights } from "./appInsights";

console.log(" App starting...");
console.log(" Environment check:", {
  speechKey: import.meta.env.VITE_AZURE_SPEECH_KEY ? "Present" : " Missing",
  openAiKey: import.meta.env.VITE_AZURE_OPENAI_KEY ? "Present" : " Missing",
  appInsightsConnectionString: import.meta.env.VITE_APPINSIGHTS_CONNECTION_STRING ? " Present" : " Missing",
  endpoint: import.meta.env.VITE_AZURE_OPENAI_ENDPOINT,
});

// Monitor Application Insights network requests
const originalFetch = window.fetch;
window.fetch = function(...args) {
  const url = args[0].toString();
  if (url.includes('applicationinsights.azure.com')) {
    console.log(" Application Insights request:", url);
    return originalFetch.apply(this, args)
      .then(response => {
        if (!response.ok) {
          console.error(" Application Insights error:", {
            status: response.status,
            statusText: response.statusText,
            url: url
          });
          return response.text().then(text => {
            console.error(" Application Insights error body:", text);
            throw new Error(`Application Insights error: ${response.status} ${response.statusText}`);
          });
        }
        console.log(" Application Insights request successful");
        return response;
      })
      .catch(error => {
        console.error(" Application Insights network error:", error);
        throw error;
      });
  }
  return originalFetch.apply(this, args);
};

console.log(" Loading Application Insights...");
try {
  appInsights.loadAppInsights();
  console.log(" Application Insights loaded successfully");
} catch (error) {
  console.error(" Application Insights failed to load:", error);
}

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <I18nTranslateProvider>
      <ReactQueryProvider>
        <AuthProvider>
          <AppInsightsErrorBoundary boundaryName="FrontendRootApp">
            <App />
          </AppInsightsErrorBoundary>
        </AuthProvider>
      </ReactQueryProvider>
    </I18nTranslateProvider>
  </StrictMode>,
);
