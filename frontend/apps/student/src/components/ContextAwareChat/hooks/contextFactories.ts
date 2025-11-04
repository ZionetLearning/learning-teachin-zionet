import { useMemo } from "react";
import type { PageContext } from "../types";

/**
 * factory functions for creating common page context patterns
 * each factory handles a specific game/practice type
 */

interface BaseGameContext {
  currentExercise: number;
  totalExercises: number;
  difficulty?: string;
  additionalContext?: Record<string, unknown>;
}

/**
 * factory for word order game contexts
 */
export const useWordOrderContext = ({
  currentExercise,
  totalExercises,
  difficulty,
  targetSentence,
  availableWords,
  userAnswer,
  additionalContext,
}: BaseGameContext & {
  targetSentence?: string;
  availableWords?: string[];
  userAnswer?: string[];
}): PageContext => {
  return useMemo(
    () => ({
      pageName: "Word Order Game",
      exerciseType: "word-order",
      currentExercise,
      totalExercises,
      difficulty,
      gameContent: {
        targetSentence,
        availableWords,
        userAnswer,
      },
      additionalContext,
    }),
    [
      currentExercise,
      totalExercises,
      difficulty,
      targetSentence,
      availableWords,
      userAnswer,
      additionalContext,
    ],
  );
};

/**
 * factory for typing practice contexts
 */
export const useTypingPracticeContext = ({
  currentExercise,
  totalExercises,
  difficulty,
  phraseToSpeak,
  userAttempt,
  correctAnswer,
  additionalContext,
}: BaseGameContext & {
  phraseToSpeak?: string;
  userAttempt?: string;
  correctAnswer?: string;
}): PageContext => {
  return useMemo(
    () => ({
      pageName: "Typing Practice",
      exerciseType: "typing",
      currentExercise,
      totalExercises,
      difficulty,
      gameContent: {
        phraseToSpeak,
        userAttempt,
        correctAnswer,
      },
      additionalContext,
    }),
    [
      currentExercise,
      totalExercises,
      difficulty,
      phraseToSpeak,
      userAttempt,
      correctAnswer,
      additionalContext,
    ],
  );
};

/**
 * factory for speaking practice contexts
 */
export const useSpeakingPracticeContext = ({
  currentExercise,
  totalExercises,
  difficulty,
  phraseToSpeak,
  additionalContext,
}: BaseGameContext & {
  phraseToSpeak?: string;
}): PageContext => {
  return useMemo(
    () => ({
      pageName: "Speaking Practice",
      exerciseType: "speaking",
      currentExercise,
      totalExercises,
      difficulty,
      gameContent: {
        phraseToSpeak,
      },
      additionalContext,
    }),
    [
      currentExercise,
      totalExercises,
      difficulty,
      phraseToSpeak,
      additionalContext,
    ],
  );
};

/**
 * factory for word cards challenge contexts
 */
export const useWordCardsContext = ({
  currentExercise,
  totalExercises,
  question,
  correctAnswer,
  userAttempt,
  currentWord,
  additionalContext,
}: BaseGameContext & {
  question?: string;
  correctAnswer?: string;
  userAttempt?: string;
  currentWord?: {
    hebrew?: string;
    english?: string;
  };
}): PageContext => {
  return useMemo(
    () => ({
      pageName: "Word Cards Challenge",
      exerciseType: "word-cards",
      currentExercise,
      totalExercises,
      gameContent: {
        question,
        correctAnswer,
        userAttempt,
        currentWord,
      },
      additionalContext,
    }),
    [
      currentExercise,
      totalExercises,
      question,
      correctAnswer,
      userAttempt,
      currentWord,
      additionalContext,
    ],
  );
};
