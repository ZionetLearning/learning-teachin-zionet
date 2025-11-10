import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient as axios } from "@app-providers";
import { isAxiosError } from "axios";
import { toast } from "react-toastify";
import { GameDifficulty } from "./game";

export type GameName = "WordOrder" | "TypingPractice" | "SpeakingPractice";

type GameConfigApiDifficulty = "Easy" | "Medium" | "Hard";

type GameConfigApiResponse = {
  gameName: GameName;
  difficulty: GameConfigApiDifficulty;
  nikud: boolean;
  numberOfSentences: number;
};

export type GameConfig = {
  gameName: GameName;
  difficulty: GameDifficulty;
  nikud: boolean;
  numberOfSentences: number;
};

export type GameConfigResponse = GameConfig;

const apiDifficultyToFrontend = (
  apiDifficulty: GameConfigApiDifficulty,
): GameDifficulty => {
  switch (apiDifficulty) {
    case "Easy":
      return "easy";
    case "Medium":
      return "medium";
    case "Hard":
      return "hard";
    default:
      return "medium";
  }
};

const frontendDifficultyToApi = (
  difficulty: GameDifficulty,
): GameConfigApiDifficulty => {
  switch (difficulty) {
    case "easy":
      return "Easy";
    case "medium":
      return "Medium";
    case "hard":
      return "Hard";
    default:
      return "Medium";
  }
};

export const useGetGameConfig = (gameName: GameName) => {
  const GAME_CONFIG_MANAGER_URL = import.meta.env.VITE_GAME_CONFIG_MANAGER_URL;

  return useQuery<GameConfigResponse, Error>({
    queryKey: ["gameConfig", gameName],
    queryFn: async () => {
      const res = await axios.get<GameConfigApiResponse>(
        `${GAME_CONFIG_MANAGER_URL}/${gameName}`,
      );
      return {
        ...res.data,
        difficulty: apiDifficultyToFrontend(res.data.difficulty),
      };
    },
    staleTime: Infinity,
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
      const apiConfig: GameConfigApiResponse = {
        ...config,
        difficulty: frontendDifficultyToApi(config.difficulty),
      };
      const res = await axios.put<GameConfigApiResponse>(
        GAME_CONFIG_MANAGER_URL,
        apiConfig,
      );
      return {
        ...res.data,
        difficulty: apiDifficultyToFrontend(res.data.difficulty),
      };
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
