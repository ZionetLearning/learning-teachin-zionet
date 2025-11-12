import { useState, useEffect, useCallback } from "react";
import {
  useGetGameConfig,
  useUpsertGameConfig,
  GameName,
  GameDifficulty,
} from "../api";
import { DifficultyLevel } from "@student/types";

export interface LocalGameConfig {
  difficulty: DifficultyLevel;
  nikud: boolean;
  count: number;
}

const difficultyLevelToApi = (level: DifficultyLevel): GameDifficulty => {
  switch (level) {
    case 0:
      return "easy";
    case 1:
      return "medium";
    case 2:
      return "hard";
    default:
      return "medium";
  }
};

const difficultyApiToLevel = (difficulty: GameDifficulty): DifficultyLevel => {
  switch (difficulty) {
    case "easy":
      return 0;
    case "medium":
      return 1;
    case "hard":
      return 2;
    default:
      return 1;
  }
};

export const useGameConfig = (gameName: GameName) => {
  const [localConfig, setLocalConfig] = useState<LocalGameConfig | null>(null);
  const { data: apiConfig, isLoading } = useGetGameConfig(gameName);
  const { mutate: saveConfig } = useUpsertGameConfig();

  useEffect(
    function loadConfig() {
      if (apiConfig && !localConfig) {
        setLocalConfig({
          difficulty: difficultyApiToLevel(apiConfig.difficulty),
          nikud: apiConfig.nikud,
          count: apiConfig.numberOfSentences,
        });
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [apiConfig],
  );

  const updateConfig = useCallback(
    (config: LocalGameConfig) => {
      setLocalConfig(config);
      saveConfig({
        gameName,
        difficulty: difficultyLevelToApi(config.difficulty),
        nikud: config.nikud,
        numberOfSentences: config.count,
      });
    },
    [gameName, saveConfig],
  );

  return {
    config: localConfig,
    isLoading,
    updateConfig,
    setConfig: setLocalConfig,
  };
};
