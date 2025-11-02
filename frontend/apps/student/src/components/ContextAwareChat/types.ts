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
}
