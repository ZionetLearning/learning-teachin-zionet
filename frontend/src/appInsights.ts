import {
  ApplicationInsights,
  type ITelemetryPlugin,
} from "@microsoft/applicationinsights-web";
import { ReactPlugin } from "@microsoft/applicationinsights-react-js";
import { createBrowserHistory } from "history";

const browserHistory = createBrowserHistory();
const reactPlugin = new ReactPlugin();

// Debug logging for Application Insights
const connectionString = import.meta.env.VITE_APPINSIGHTS_CONNECTION_STRING;
console.log("📊 Application Insights Debug:", {
  hasConnectionString: !!connectionString,
  connectionStringLength: connectionString?.length || 0,
  connectionStringStart: connectionString?.substring(0, 30) + "..." || "undefined",
  status: connectionString ? "✅ ENABLED - Using Terraform-provided connection string" : "⚠️ DISABLED - No connection string"
});

// Enable Application Insights with proper configuration
const appInsights = new ApplicationInsights({
  config: {
    connectionString: connectionString,
    enableAutoRouteTracking: true,
    enableRequestHeaderTracking: false,
    enableResponseHeaderTracking: false,
    enableAjaxErrorStatusText: false,
    enableAjaxPerfTracking: false,
    enableCorsCorrelation: false,
    enableUnhandledPromiseRejectionTracking: true,
    loggingLevelTelemetry: 0,
    loggingLevelConsole: 1,
    disableExceptionTracking: false,
    disableAjaxTracking: false, // Re-enable with correct workspace
    extensions: [reactPlugin as unknown as ITelemetryPlugin],
    extensionConfig: {
      [reactPlugin.identifier]: {
        history: browserHistory,
      },
    },
  },
});

// Override telemetry to prevent invalid workspace errors
const originalLoadAppInsights = appInsights.loadAppInsights.bind(appInsights);
(appInsights as any).loadAppInsights = function(...args: any[]) {
  try {
    return originalLoadAppInsights(...args);
  } catch (error) {
    console.warn("⚠️ Application Insights failed to load, continuing without telemetry:", error);
    return appInsights; // Return the instance to satisfy the return type
  }
};

export { appInsights, reactPlugin };
