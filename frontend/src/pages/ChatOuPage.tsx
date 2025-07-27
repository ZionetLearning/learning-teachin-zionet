import { useEffect } from "react";
import * as Sentry from "@sentry/react";
import { ChatOu } from "../features";
import { initializeSentry } from "../utils/sentry";

export const ChatOuPage = () => {
  useEffect(() => {
    initializeSentry();
  }, []);

  return (
    <Sentry.ErrorBoundary fallback={<div>Something went wrong</div>}>
      <ChatOu />
    </Sentry.ErrorBoundary>
  );
};
