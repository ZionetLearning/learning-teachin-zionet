import { useCallback, useEffect, useRef, useState } from "react";
import { useRequestWordExplanation } from "../api";
import { useSignalR } from "./useSignalR";
import { EventType, type WordExplanationResponse } from "@app-providers";

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
                30000,
              );
              setExplanation(response.explanation);
            } catch (error) {
              console.error("Failed to receive explanation:", error);
              requestKeyRef.current = "";
            }
          },
          onError: () => {
            requestKeyRef.current = "";
          },
        },
      );
    },
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
