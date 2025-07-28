import React from "react";
import { appInsights } from "../../appInsights";

interface Props {
  children: React.ReactNode; // the components this boundary wraps
  boundaryName?: string; // optional name to identify this boundary in telemetry
  fallback?: React.ReactNode; // optional fallback UI
}

interface State {
  hasError: boolean;
}

export class AppInsightsErrorBoundary extends React.Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }

  // lifecycle method called when a child component throws an error
  static getDerivedStateFromError(): State {
    return { hasError: true };
  }

  // lifecycle method called after an error has been thrown (log the error to Azure Application Insights)
  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error(
      "AppInsightsErrorBoundary caught an error:",
      error,
      errorInfo
    );

    appInsights.trackException({
      exception: error, //the error object
      severityLevel: 3,  //error level 
      properties: {
        boundary: this.props.boundaryName ?? "AppInsightsErrorBoundary",
        componentStack: errorInfo.componentStack, //shows the component that threw the error
      },
    });
  }

  render() {
    if (this.state.hasError) {
      return this.props.fallback ?? <h1>Something went wrong...</h1>;
    }

    return this.props.children;
  }
}
