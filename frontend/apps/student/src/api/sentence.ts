import { useMutation } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import { toast } from "react-toastify";
import {
  EventType,
  SplitSentenceGeneratedPayload,
  UserEventUnion,
} from "@app-providers";

import { useSignalR } from "@student/hooks";

export type SentenceRequest = {
  difficulty: 0 | 1 | 2;
  nikud: boolean;
  count: number;
};

// Hook for fetching split sentences
export const useGenerateSplitSentences = () => {
  const AI_BASE_URL = import.meta.env.VITE_AI_URL!;
  const { subscribe } = useSignalR();

  return useMutation<SplitSentenceGeneratedPayload, Error, SentenceRequest>({
    mutationFn: async ({ difficulty, nikud, count }) => {
      const requestBody = {
        difficulty,
        nikud,
        count,
      };

      let timeout: NodeJS.Timeout;
      let unsubscribe: (() => void) | undefined;

      // Set up a promise to wait for the SignalR response
      const responsePromise = new Promise<SplitSentenceGeneratedPayload>(
        (resolve, reject) => {
          timeout = setTimeout(() => {
            reject(new Error("Timeout waiting for split sentence response"));
          }, 30000);

          // Subscribe to SignalR events
          unsubscribe = subscribe("ReceiveEvent", (event: UserEventUnion) => {
            // Check if this is our split sentence event
            if (event.eventType === EventType.SplitSentenceGeneration) {
              // Clean up when we get our response
              clearTimeout(timeout);
              unsubscribe?.();
              resolve(event.payload);
            }
          });
        },
      );

      // Make the API call
      const response = await axios.post(
        `${AI_BASE_URL}/sentence/split`,
        requestBody,
      );

      const splitSentenceResponse = await responsePromise;
      return splitSentenceResponse;
    },

    onError: (error) => {
      console.error("Failed to fetch split sentences:", error);
      toast.error("Failed to fetch split sentences. Please try again.");
    },
  });
};