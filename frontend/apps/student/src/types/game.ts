export const GameType = {
  WordOrderGame: "WordOrderGame",
  TypingPractice: "TypingPractice",
  SpeakingPractice: "SpeakingPractice",
} as const;

export type GameType = (typeof GameType)[keyof typeof GameType];
