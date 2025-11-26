import { useQuery } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import type { Achievement } from "../types/achievement";

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
