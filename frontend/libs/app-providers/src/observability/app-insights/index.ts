import { ApplicationInsights } from "@microsoft/applicationinsights-web";
import { ITelemetryItem } from "@microsoft/applicationinsights-core-js";

let ai: ApplicationInsights | null = null;

type TelemetryProps = Record<string, unknown>;

interface BaseDataWithProps {
  properties?: TelemetryProps;
}

interface TelemetryItemWithProps extends ITelemetryItem {
  data?: {
    baseData?: BaseDataWithProps;
  };
}

export function hasBaseDataWithProps(
  item: ITelemetryItem,
): item is TelemetryItemWithProps {
  return (
    typeof (item as TelemetryItemWithProps).data === "object" &&
    !!(item as TelemetryItemWithProps).data &&
    typeof (item as TelemetryItemWithProps).data!.baseData === "object"
  );
}

export const initAppInsights = (appName: string) => {
  if (ai) return ai;

  const cs = import.meta.env.VITE_APPINSIGHTS_CONNECTION_STRING;
  if (!cs) {
    console.warn("[AI] Missing VITE_APPINSIGHTS_CONNECTION_STRING");
    return null;
  }

  ai = new ApplicationInsights({
    config: {
      correlationHeaderExcludedDomains: ["api.jikan.moe"],
      connectionString: cs,
      enableAutoRouteTracking: true,
      enableCorsCorrelation: true,
      enableRequestHeaderTracking: true,
      enableResponseHeaderTracking: true,
      samplingPercentage: 100,
    },
  });

  ai.loadAppInsights();

  ai.addTelemetryInitializer((envelope: ITelemetryItem) => {
    envelope.tags = envelope.tags ?? {};
    envelope.tags["ai.cloud.role"] = appName;
    envelope.tags["ai.cloud.roleInstance"] = appName;

    if (hasBaseDataWithProps(envelope) && envelope.data?.baseData) {
      envelope.data.baseData.properties = {
        ...(envelope.data.baseData.properties ?? {}),
        appName,
      };
    }
  });

  ai.trackPageView();

  return ai;
};
