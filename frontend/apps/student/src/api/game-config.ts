import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import { isAxiosError } from "axios";
import { toast } from "react-toastify";
import { GameDifficulty } from "./game";

export type GameName = "WordOrder" | "TypingPractice" | "SpeakingPractice";

export type GameConfig = {
  gameName: GameName;
  difficulty: GameDifficulty;
  nikud: boolean;
  numberOfSentences: number;
};

export type GameConfigResponse = GameConfig;

export const useGetGameConfig = (gameName: GameName) => {
  const GAME_CONFIG_MANAGER_URL = import.meta.env.VITE_GAME_CONFIG_MANAGER_URL;

  return useQuery<GameConfigResponse, Error>({
    queryKey: ["gameConfig", gameName],
    queryFn: async () => {
      const res = await axios.get<GameConfigResponse>(
        `${GAME_CONFIG_MANAGER_URL}/${gameName}`,
      );
      return res.data;
    },
    staleTime: 300_000, // 5 minutes
    retry: (failureCount, error) => {
      if (isAxiosError(error) && error.response?.status === 404) {
        return false;
      }
      return failureCount < 3;
    },
  });
};

export const useUpsertGameConfig = () => {
  const GAME_CONFIG_MANAGER_URL = import.meta.env.VITE_GAME_CONFIG_MANAGER_URL;
  const queryClient = useQueryClient();

  return useMutation<GameConfigResponse, Error, GameConfig>({
    mutationFn: async (config: GameConfig) => {
      const res = await axios.put<GameConfigResponse>(
        GAME_CONFIG_MANAGER_URL,
        config,
      );
      return res.data;
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({
        queryKey: ["gameConfig", data.gameName],
      });
      toast.success("Game configuration saved successfully");
    },
    onError: (error) => {
      console.error("Failed to save game configuration:", error);
      toast.error("Failed to save game configuration. Please try again.");
    },
  });
};

export const useDeleteGameConfig = () => {
  const GAME_CONFIG_MANAGER_URL = import.meta.env.VITE_GAME_CONFIG_MANAGER_URL;
  const queryClient = useQueryClient();

  return useMutation<void, Error, GameName>({
    mutationFn: async (gameName: GameName) => {
      await axios.delete(`${GAME_CONFIG_MANAGER_URL}/${gameName}`);
    },
    onSuccess: (_, gameName) => {
      queryClient.invalidateQueries({ queryKey: ["gameConfig", gameName] });
      toast.success("Game configuration deleted successfully");
    },
    onError: (error) => {
      console.error("Failed to delete game configuration:", error);
      toast.error("Failed to delete game configuration. Please try again.");
    },
  });
};
