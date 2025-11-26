import {
  useQuery,
  useMutation,
  useQueryClient,
  UseMutationResult,
} from "@tanstack/react-query";
import { toast } from "react-toastify";
import { apiClient as axios } from "@app-providers";
import type {
  Achievement,
  TrackProgressRequest,
  TrackProgressResponse,
} from "../types/achievement";

const ACHIEVEMENTS_URL = import.meta.env.VITE_ACHIEVEMENTS_MANAGER_URL;
const ACHIEVEMENTS_STALE_TIME = 1000 * 60 * 5; // 5 minutes

export const useGetUserAchievements = (userId: string | undefined) => {
  return useQuery<Achievement[], Error>({
    queryKey: ["achievements", userId],
    queryFn: async () => {
      if (!userId) throw new Error("Missing userId");

      const { data } = await axios.get<Achievement[]>(
        `${ACHIEVEMENTS_URL}/user/${userId}`,
      );
      return data;
    },
    enabled: !!userId,
    staleTime: ACHIEVEMENTS_STALE_TIME,
    refetchOnWindowFocus: true,
  });
};

export const useTrackProgress = (): UseMutationResult<
  TrackProgressResponse,
  Error,
  TrackProgressRequest
> => {
  const queryClient = useQueryClient();

  return useMutation<TrackProgressResponse, Error, TrackProgressRequest>({
    mutationFn: async (request: TrackProgressRequest) => {
      const { data } = await axios.post<TrackProgressResponse>(
        `${ACHIEVEMENTS_URL}/track`,
        request,
      );
      return data;
    },
    onSuccess: (response, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["achievements", variables.userId],
      });

      if (response.unlockedAchievements.length > 0) {
        console.log("Unlocked achievements:", response.unlockedAchievements);
      }
    },
    onError: (error) => {
      console.error("Failed to track progress:", error);
      toast.error("Failed to track progress");
    },
  });
};
