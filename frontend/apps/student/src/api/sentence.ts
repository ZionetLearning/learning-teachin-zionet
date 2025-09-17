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

  type SentenceRequestWire = {
    difficulty: number;   // <-- enum as number for the server
    nikud: boolean;
    count: number;
  };

  const toWire = (p: SentenceRequest): SentenceRequestWire => ({
    difficulty: p.difficulty === 'easy' ? 0 : p.difficulty === 'medium' ? 1 : 2,
    nikud: p.nikud,
    count: p.count,
  });

  export const usePostSentence = () => {
    const AI_BASE_URL = import.meta.env.VITE_AI_URL!;
  
    return useMutation<void, Error, SentenceRequest>({
      mutationFn: async (payload) => {
        await axios.post(`${AI_BASE_URL}/sentence`, toWire(payload), {
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
