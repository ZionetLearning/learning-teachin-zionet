import { ApplicationInsights } from '@microsoft/applicationinsights-web';

let ai: ApplicationInsights | null = null;

export function initAppInsights(appName: string) {
  if (ai) return ai;

  const cs = import.meta.env.VITE_APPINSIGHTS_CONNECTION_STRING;
  if (!cs) {
    console.warn('[AI] Missing VITE_APPINSIGHTS_CONNECTION_STRING');
    return null;
  }

  ai = new ApplicationInsights({
    config: {
      connectionString: cs,
      enableAutoRouteTracking: true,
      enableCorsCorrelation: true,
      enableRequestHeaderTracking: true,
      enableResponseHeaderTracking: true,
      samplingPercentage: 100
    }
  });

  ai.loadAppInsights();

  ai.addTelemetryInitializer((envelope) => {
    envelope.tags = envelope.tags || [];
    envelope.tags["ai.cloud.role"] = appName;
    envelope.tags["ai.cloud.roleInstance"] = appName;

    const anyEnv = envelope as any;
    if (anyEnv.data?.baseData) {
      anyEnv.data.baseData.properties = {
        ...(anyEnv.data.baseData.properties || {}),
        appName
      };
    }
  });

  ai.trackPageView();

  return ai;
}