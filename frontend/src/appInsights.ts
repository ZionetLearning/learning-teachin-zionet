import {
  ApplicationInsights,
  type ITelemetryPlugin,
} from "@microsoft/applicationinsights-web";
import { ReactPlugin } from "@microsoft/applicationinsights-react-js";
import { createBrowserHistory } from "history";

const browserHistory = createBrowserHistory();
const reactPlugin = new ReactPlugin();

const appInsights = new ApplicationInsights({
  config: {
    connectionString: import.meta.env.VITE_APPINSIGHTS_CONNECTION_STRING,
    enableAutoRouteTracking: true,
    extensions: [reactPlugin as unknown as ITelemetryPlugin],
    extensionConfig: {
      [reactPlugin.identifier]: {
        history: browserHistory,
      },
    },
  },
});

export { appInsights, reactPlugin };
