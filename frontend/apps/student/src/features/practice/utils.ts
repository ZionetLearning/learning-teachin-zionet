import { DifficultyLevel } from "@student/types";

export const getDifficultyLabel = (
  difficulty: DifficultyLevel,
  t: (key: string) => string,
) => {
  switch (difficulty) {
    case 0:
      return t("pages.wordOrderGame.difficulty.easy");
    case 1:
      return t("pages.wordOrderGame.difficulty.medium");
    case 2:
      return t("pages.wordOrderGame.difficulty.hard");
    default:
      return t("pages.wordOrderGame.difficulty.medium");
  }
};
