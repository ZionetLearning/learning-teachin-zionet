import {
  useMutation,
  useQuery,
  UseQueryResult,
  useQueryClient,
} from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import { toast } from "react-toastify";

export type WordCard = {
  cardId: string;
  hebrew: string;
  english: string;
  isLearned: boolean;
  explanation?: string;
};

export type CreateWordCardRequest = {
  hebrew: string;
  english: string;
  context?: string;
  explanation?: string;
};

export type WordExplainRequest = {
  word: string;
  context: string;
};

export type SetLearnedRequest = {
  cardId: string;
  isLearned: boolean;
};

const WORD_CARDS_MANAGER_URL = import.meta.env.VITE_WORD_CARDS_MANAGER_URL!;
const AI_MANAGER_URL = import.meta.env.VITE_AI_URL!;

export const useGetWordCards = (): UseQueryResult<WordCard[], Error> => {
  return useQuery<WordCard[], Error>({
    queryKey: ["wordcards"],
    queryFn: async () => {
      const res = await axios.get<WordCard[]>(WORD_CARDS_MANAGER_URL);
      return res.data;
    },
  });
};

export const useCreateWordCard = () => {
  const qc = useQueryClient();

  return useMutation<WordCard, Error, CreateWordCardRequest>({
    mutationFn: async (body) => {
      const res = await axios.post<WordCard>(WORD_CARDS_MANAGER_URL, body);
      return res.data;
    },
    onSuccess: () => {
      toast.success("Word added successfully!");
      qc.invalidateQueries({ queryKey: ["wordcards"] });
    },
    onError: (error) => {
      console.error("Failed to create word card:", error);
      toast.error("Couldn’t add the word. Please try again.");
    },
  });
};

export const useSetWordCardLearned = () => {
  const qc = useQueryClient();

  return useMutation<
    { cardId: string; isLearned: boolean },
    Error,
    SetLearnedRequest
  >({
    mutationFn: async ({ cardId, isLearned }) => {
      const res = await axios.patch<{ cardId: string; isLearned: boolean }>(
        `${WORD_CARDS_MANAGER_URL}/learned`,
        { cardId, isLearned },
      );
      return res.data;
    },
    onError: (error) => {
      console.error("Failed to set learned:", error);
      toast.error("Couldn’t update card status.");
    },
    onSettled: () => {
      qc.invalidateQueries({ queryKey: ["wordcards"] });
    },
  });
};

/**
 * Requests an AI-generated explanation for a word in context.
 * @returns A request ID that can be used to track the response via SignalR
 */
export const useRequestWordExplanation = () => {
  return useMutation<string, Error, WordExplainRequest>({
    mutationFn: async (body) => {
      const res = await axios.post<string>(
        `${AI_MANAGER_URL}/word-explain`,
        body,
      );
      return res.data;
    },
    onError: (error) => {
      console.error("Failed to request word explanation:", error);
      toast.error("Couldn't request explanation. Please try again.");
    },
  });
};
