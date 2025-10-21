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

const WORD_CARDS_MANAGER_URL = import.meta.env.VITE_WORD_CARDS_MANAGER_URL!;

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
        //shahar is working on deleted the cardId from the url
        `${WORD_CARDS_MANAGER_URL}/${encodeURIComponent(cardId)}/learned`,
        { isLearned, cardId },
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
