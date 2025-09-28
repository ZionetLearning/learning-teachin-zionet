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

export const stripHebrewNikud = (input: string): string => {
  // Normalize, then remove Hebrew diacritics & cantillation marks
  const noMarks = input.normalize("NFKD").replace(/[\u0591-\u05C7]/g, "");
  return noMarks.replace(/[\u200e\u200f]/g, "");
};

export const splitGraphemes = (text: string): string[] => {
  if ("Segmenter" in Intl) {
    const segmenter = new Intl.Segmenter("he", { granularity: "grapheme" });
    return Array.from(segmenter.segment(text), (s) => s.segment);
  }
  return Array.from(text); // fallback
};
