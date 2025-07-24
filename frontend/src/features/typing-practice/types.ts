export interface Exercise {
    id: string;
    hebrewText: string;
    difficulty: 'easy' | 'medium' | 'hard';
}

export interface ExerciseBank {
    easy: Exercise[];
    medium: Exercise[];
    hard: Exercise[];
}

export interface ExerciseState {
    phase: 'level-selection' | 'ready' | 'playing' | 'typing' | 'feedback';
    selectedLevel: 'easy' | 'medium' | 'hard' | null;
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
}

export type DifficultyLevel = 'easy' | 'medium' | 'hard';
