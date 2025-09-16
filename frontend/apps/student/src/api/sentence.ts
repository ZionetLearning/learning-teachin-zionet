import { useMutation } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import { toast } from "react-toastify";

export const Difficulty = {
    easy: "easy",
    medium: "medium",
    hard: "hard",
  } as const;
  export type Difficulty = typeof Difficulty[keyof typeof Difficulty];
  
  export type SentenceRequest = {
    difficulty: Difficulty;
    nikud: boolean;
    count: number;
  };
  
  export const usePostSentence = () => {
    const AI_BASE_URL = import.meta.env.VITE_AI_URL!;
  
    return useMutation<void, Error, SentenceRequest>({
      mutationFn: async (payload) => {
        await axios.post(`${AI_BASE_URL}/sentence`, payload, {
          headers: { "Content-Type": "application/json" },
        });
      },
      onSuccess: () => toast.success("Sentence request accepted"),
      onError: (err) => {
        console.error("POST /sentence failed:", err);
        toast.error("Failed to post sentence request");
      },
    });
  };
