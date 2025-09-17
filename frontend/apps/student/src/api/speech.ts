import { useQuery } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";

export type AzureSpeechTokenResponse = {
  token: string;
  region: string;
};

export const useAzureSpeechToken = () => {
  const AI_BASE_URL = import.meta.env.VITE_MEDIA_URL!;
  return useQuery<AzureSpeechTokenResponse, Error>({
    queryKey: ["azureSpeechToken"],
    queryFn: async () => {
      const res = await axios.get<AzureSpeechTokenResponse>(
        `${AI_BASE_URL}/speech/token`,
      );
      return res.data;
    },
    staleTime: 540000, // 9 minutes
    refetchInterval: 540000, // 9 minutes
    refetchOnWindowFocus: false,
    retry: 2,
  });
};
