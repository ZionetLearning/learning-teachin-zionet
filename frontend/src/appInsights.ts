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
console.log(" Application Insights Debug:", {
  hasConnectionString: !!connectionString,
  connectionStringLength: connectionString?.length || 0,
  connectionStringStart: connectionString?.substring(0, 30) + "..." || "undefined",
});

const appInsights = new ApplicationInsights({
  config: {
    connectionString: connectionString,
    enableAutoRouteTracking: true,
    enableRequestHeaderTracking: true,
    enableResponseHeaderTracking: true,
    enableAjaxErrorStatusText: true,
    enableAjaxPerfTracking: true,
    enableCorsCorrelation: true,
    enableUnhandledPromiseRejectionTracking: true,
    loggingLevelTelemetry: 2, // Verbose logging
    loggingLevelConsole: 2, // Verbose console logging
    extensions: [reactPlugin as unknown as ITelemetryPlugin],
    extensionConfig: {
      [reactPlugin.identifier]: {
        history: browserHistory,
      },
    },
  },
});

// Add error tracking
appInsights.addTelemetryInitializer((envelope) => {
  console.log(" Sending telemetry:", envelope);
  return true;
});

export { appInsights, reactPlugin };
