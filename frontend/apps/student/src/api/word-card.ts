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
};

export type CreateWordCardRequest = {
  hebrew: string;
  english: string;
};

export type SetLearnedRequest = {
  cardId: string;
  isLearned: boolean;
};

const BASE_URL = import.meta.env.VITE_BASE_URL!;

export const useGetWordCards = (): UseQueryResult<WordCard[], Error> => {
  return useQuery<WordCard[], Error>({
    queryKey: ["wordcards"],
    queryFn: async () => {
      const res = await axios.get<WordCard[]>(`${BASE_URL}/wordcards`);
      return res.data;
    },
    staleTime: 60_000,
  });
};

export const useCreateWordCard = () => {
  const qc = useQueryClient();

  return useMutation<WordCard, Error, CreateWordCardRequest>({
    mutationFn: async (body) => {
      const res = await axios.post<WordCard>(`${BASE_URL}/wordcards`, body);
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
        `${BASE_URL}/wordcards/${encodeURIComponent(cardId)}/learned`,
        { isLearned },
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
