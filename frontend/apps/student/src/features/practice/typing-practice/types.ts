export interface ExerciseState {
  phase: "level-selection" | "ready" | "playing" | "typing" | "feedback";
  selectedLevel: "easy" | "medium" | "hard" | null;
  isLoading: boolean;
  error: string | null;
  audioState: {
    isPlaying: boolean;
    hasPlayed: boolean;
    error: string | null;
  };
  userInput: string;
  feedbackResult: FeedbackResult | null;
}

export interface FeedbackResult {
  userInput: string;
  expectedText: string;
  accuracy: number;
  characterComparison: Array<{
    char: string;
    isCorrect: boolean;
    position: number;
  }>;
  isCorrect: boolean;
}

export type DifficultyLevel = "easy" | "medium" | "hard";
