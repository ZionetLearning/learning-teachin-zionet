import { useMutation, useQuery } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";

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

export type AzureSpeechTokenResponse = {
  token: string;
  region?: string;
};

//this hook returns a mutation function that takes in text and returns synthesized speech data
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

export const useAzureSpeechToken = () => {
  const AI_BASE_URL = import.meta.env.VITE_MEDIA_URL!;
  return useQuery<AzureSpeechTokenResponse, Error>({
    queryKey: ["azureSpeechToken"],
    queryFn: async () => {
      const res = await axios.get<AzureSpeechTokenResponse>(`${AI_BASE_URL}/speech/token`);
      return res.data;
    },
    staleTime: 540000, // 9 minutes
    refetchInterval: 540000, // 9 minutes
    // refetchOnWindowFocus: false, // avoid surprise re-fetches
    // retry: 2, // soften transient network hiccups
  });
};
