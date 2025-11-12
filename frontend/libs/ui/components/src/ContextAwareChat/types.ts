export interface ChatMessage {
  id: string;
  text: string;
  sender: "user" | "assistant";
  timestamp: Date;
}

export interface PageContext {
  pageName: string;
  exerciseType?: string;
  currentExercise?: number;
  totalExercises?: number;
  difficulty?: string;
  additionalContext?: Record<string, unknown>;
  // game-specific content
  gameContent?: {
    // for word-order game
    targetSentence?: string;
    targetTranslation?: string;
    availableWords?: string[];
    userAnswer?: string[];
    // for speaking practice
    phraseToSpeak?: string;
    phraseTranslation?: string;
    // for word cards
    currentWord?: {
      hebrew?: string;
      english?: string;
      translation?: string;
    };
    // for any game
    question?: string;
    options?: string[];
    correctAnswer?: string;
    userAttempt?: string;
  };
}
