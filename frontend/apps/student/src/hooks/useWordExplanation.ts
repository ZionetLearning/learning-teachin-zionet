import { useCallback, useEffect, useRef, useState } from "react";
import { useRequestWordExplanation } from "../api";
import { useSignalR } from "./useSignalR";
import { EventType, type WordExplanationResponse } from "@app-providers";

const REQUEST_TIMEOUT = 30000; // 30 seconds

export const useWordExplanation = (
  word: string,
  context: string,
  enabled: boolean,
) => {
  const { waitForResponse } = useSignalR();
  const requestExplanation = useRequestWordExplanation();
  const [explanation, setExplanation] = useState<string>("");
  const requestKeyRef = useRef<string>("");

  useEffect(
    function fetchExplanation() {
      const requestKey = `${word}:${context}:${enabled}`;
      let cancelled = false;

      if (
        !enabled ||
        !word ||
        !context ||
        requestKeyRef.current === requestKey
      ) {
        return;
      }

      requestKeyRef.current = requestKey;

      requestExplanation.mutate(
        { word, context },
        {
          onSuccess: async (requestId) => {
            try {
              const response = await waitForResponse<WordExplanationResponse>(
                EventType.WordExplain,
                requestId,
                REQUEST_TIMEOUT,
              );

              if (!cancelled) {
                setExplanation(response.explanation);
              }
            } catch (error) {
              if (!cancelled) {
                console.error("Failed to receive explanation:", error);
                requestKeyRef.current = "";
              }
            }
          },
          onError: () => {
            if (!cancelled) {
              requestKeyRef.current = "";
            }
          },
        },
      );

      return () => {
        cancelled = true;
      };
    },
    // requestExplanation.mutate and waitForResponse are stable functions
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [enabled, word, context],
  );

  const reset = useCallback(() => {
    setExplanation("");
    requestKeyRef.current = "";
  }, []);

  return {
    explanation,
    isLoading: requestExplanation.isPending,
    reset,
  };
};
