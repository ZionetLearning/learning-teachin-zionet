import { useMutation } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import { toast } from "react-toastify";
import {
  EventType,
  SentenceItem,
  SplitSentenceItem,
  SentenceGenerationResponse,
  SplitSentenceGenerationResponse,
} from "@app-providers/types";

import { useSignalR } from "@student/hooks";
import { GameType } from "@student/types";

export type SentenceRequest = {
  difficulty: 0 | 1 | 2;
  nikud: boolean;
  count: number;
  gameType?: GameType;
};

export const useGenerateSentences = () => {
  const { waitForResponse, status } = useSignalR();

  return useMutation<SentenceItem[], Error, SentenceRequest>({
    mutationKey: ["generateSentences", status],
    mutationFn: async (requestBody) => {
      if (status !== "connected") {
        throw new Error("SignalR is not connected");
      }

      const { data } = await axios.post<{ requestId: string }>(
        `${import.meta.env.VITE_AI_URL!}/sentence`,
        requestBody,
      );

      const requestId = data.requestId;

      const response = await waitForResponse<SentenceGenerationResponse>(
        EventType.SentenceGeneration,
        requestId,
        120_000, 
      );

      return response.sentences;
    },
    onError: (error) => {
      console.error("Failed to fetch sentences:", error);
      toast.error("Failed to fetch sentences. Please try again.");
    },
  });
};

export const useGenerateSplitSentences = () => {
  const AI_BASE_URL = import.meta.env.VITE_AI_URL!;
  const { waitForResponse, status } = useSignalR();

  return useMutation<SplitSentenceItem[], Error, SentenceRequest>({
    mutationKey: ["generateSplitSentences", status],
    mutationFn: async ({ difficulty, nikud, count, gameType = GameType.WordOrderGame }) => {
      if (status !== "connected") {
        throw new Error("SignalR is not connected");
      }

      const requestBody = {
        difficulty,
        nikud,
        count,
        gameType,
      };

      const { data } = await axios.post<{ requestId: string }>(
        `${AI_BASE_URL}/sentence/split`,
        requestBody,
      );

      const requestId = data.requestId;

      const response = await waitForResponse<SplitSentenceGenerationResponse>(
        EventType.SplitSentenceGeneration,
        requestId,
        60_000, 
      );

      return response.sentences;
    },

    onError: (error) => {
      console.error("Failed to fetch split sentences:", error);
      toast.error("Failed to fetch split sentences. Please try again.");
    },
  });
};
