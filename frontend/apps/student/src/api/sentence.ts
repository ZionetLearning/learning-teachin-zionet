import { useMutation } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import { toast } from "react-toastify";
import {
  EventType,
  SentenceGeneratedPayload,
  SplitSentenceGeneratedPayload,
} from "@app-providers/types";
import { useSignalR } from "@student/hooks";

export type SentenceRequest = {
  difficulty: 0 | 1 | 2; // 0=easy, 1=medium, 2=hard
  nikud: boolean; // Hebrew diacritics
  count: number; // number of sentences to generate
};

// Re-export the types from SignalR types for convenience
export type {
  SentenceItem,
  SplitSentenceItem,
  SentenceGeneratedPayload,
  SplitSentenceGeneratedPayload,
} from "@app-providers/types";

// Hook for fetching regular sentences
export const useFetchSentences = () => {
  const AI_BASE_URL = import.meta.env.VITE_AI_URL!;
  const { subscribe } = useSignalR();

  return useMutation<SentenceGeneratedPayload, Error, SentenceRequest>({
    mutationFn: async ({ difficulty, nikud, count }) => {
      console.log("Starting sentence fetch with config:", {
        difficulty,
        nikud,
        count,
      });
      console.log("AI_BASE_URL:", AI_BASE_URL);

      const requestBody = {
        difficulty,
        nikud,
        count,
      };

      console.log("Sending request to:", `${AI_BASE_URL}/sentence`);
      console.log("Request body:", requestBody);

      // Set up a promise to wait for the SignalR response
      const responsePromise = new Promise<SentenceGeneratedPayload>(
        (resolve, reject) => {
          const timeout = setTimeout(() => {
            reject(new Error("Timeout waiting for sentence response"));
          }, 30000);

          // Subscribe to ALL SignalR events to see what we get
          const unsubscribe = subscribe("ReceiveEvent", (event: any) => {
            console.log("Received SignalR event:", event);
            console.log("Event type:", event.eventType);
            console.log("Expected type:", EventType.SentenceGeneration);
            console.log(
              "Types match:",
              event.eventType === EventType.SentenceGeneration,
            );

            // Check if this is our sentence event
            if (event.eventType === EventType.SentenceGeneration) {
              console.log("Found matching sentence event!");
              clearTimeout(timeout);
              unsubscribe();
              resolve(event.payload);
            } else {
              console.log("Event type doesn't match, still waiting...");
            }
          });
        },
      );

      // Make the API call
      const response = await axios.post(`${AI_BASE_URL}/sentence`, requestBody);
      console.log("API response:", response.status, response.data);

      console.log("Waiting for SignalR response");
      console.log("Waiting for event type:", EventType.SentenceGeneration);

      const sentenceResponse = await responsePromise;

      console.log("Received SignalR response:", sentenceResponse);
      return sentenceResponse;
    },

    onError: (error) => {
      console.error("Failed to fetch sentences:", error);
      toast.error("Failed to fetch sentences. Please try again.");
    },
  });
};

// Hook for fetching split sentences
export const useGenerateSplitSentences = () => {
  const AI_BASE_URL = import.meta.env.VITE_AI_URL!;
  const { subscribe } = useSignalR();

  return useMutation<SplitSentenceGeneratedPayload, Error, SentenceRequest>({
    mutationFn: async ({ difficulty, nikud, count }) => {
      console.log("Starting split sentence fetch with config:", {
        difficulty,
        nikud,
        count,
      });

      const requestBody = {
        difficulty,
        nikud,
        count,
      };

      console.log("Sending request to:", `${AI_BASE_URL}/sentence/split`);
      console.log("Request body:", requestBody);

      // Set up a promise to wait for the SignalR response
      const responsePromise = new Promise<SplitSentenceGeneratedPayload>(
        (resolve, reject) => {
          const timeout = setTimeout(() => {
            reject(new Error("Timeout waiting for split sentence response"));
          }, 30000);

          // Subscribe to ALL SignalR events to see what we get
          const unsubscribe = subscribe("ReceiveEvent", (event: any) => {
            console.log("Received SignalR event for split:", event);
            console.log("Event type:", event.eventType);
            console.log("Expected type:", EventType.SplitSentenceGeneration);

            // Check if this is our split sentence event
            if (event.eventType === EventType.SplitSentenceGeneration) {
              console.log("Found matching split sentence event!");
              clearTimeout(timeout);
              unsubscribe();
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
      console.log("API response:", response.status, response.data);

      console.log("Waiting for SignalR response");
      console.log("Waiting for event type:", EventType.SplitSentenceGeneration);

      const splitSentenceResponse = await responsePromise;

      console.log("Received SignalR response:", splitSentenceResponse);
      return splitSentenceResponse;
    },

    onError: (error) => {
      console.error("Failed to fetch split sentences:", error);
      toast.error("Failed to fetch split sentences. Please try again.");
    },
  });
};
