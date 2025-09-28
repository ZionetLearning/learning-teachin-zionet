

import * as Sentry from "@sentry/react";
import { ErrorFallback, ErrorFallbackProps } from "@ui-components";

interface SentryErrorBoundaryProps {
  children: React.ReactNode;
  fallback?: (props: ErrorFallbackProps) => React.ReactNode;
}

export const SentryErrorBoundary = ({
  children,
  fallback: FallbackComponent = ErrorFallback,
}: SentryErrorBoundaryProps) => {
  return (
    <Sentry.ErrorBoundary
      fallback={({ resetError }) => (
        <FallbackComponent
          resetErrorBoundary={resetError}
        />
      )}
    >
      {children}
    </Sentry.ErrorBoundary>
  );
};
