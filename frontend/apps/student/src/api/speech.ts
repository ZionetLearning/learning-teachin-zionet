import { useMutation } from "@tanstack/react-query";
import axios from "axios";

export type VisemeEvent = {
  visemeId: number;
  offsetMs: number;
};

export type SynthesizeResponse = {
  audioData: string;
  visemes: Array<VisemeEvent>;
  metadata: {
    audioLength: number;
    audioFormat: string;
    processingDuration: string;
  };
};

export type SynthesizerRequest = {
  text: string;
  rate?: number;
};

export const useSynthesizeSpeech = () => {
  const AI_BASE_URL = import.meta.env.VITE_AI_URL!;
  return useMutation<SynthesizeResponse, Error, SynthesizerRequest>({
    mutationFn: async ({ text }: SynthesizerRequest) => {
      const response = await axios.post<SynthesizeResponse>(
        `${AI_BASE_URL}/speech/synthesize`,
        { text },
      );
      return response.data;
    },
  });
};
