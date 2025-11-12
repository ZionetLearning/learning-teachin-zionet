import { useState, useCallback, useRef, useMemo } from "react";
import { useGenerateSentences } from "@student/api";
import { DifficultyLevel, GameType } from "@student/types";
import { SentenceItem } from "@app-providers";

export interface UseHebrewSentenceConfig {
  difficulty?: DifficultyLevel;
  nikud?: boolean;
  count?: number;
  gameType?: GameType;
}

// Create a single state object to prevent sync issues
interface SentenceState {
  exerciseId: string;
  sentence: string;
  words: string[];
}

export const useHebrewSentence = (config: UseHebrewSentenceConfig = {}) => {
  const [sentenceState, setSentenceState] = useState<SentenceState>({
    exerciseId: "",
    sentence: "",
    words: [],
  });

  const [sentencePool, setSentencePool] = useState<SentenceItem[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [error, setError] = useState<string | null>(null);

  const pendingRef = useRef(false);
  const didInitRef = useRef(false);

  const {
    mutateAsync: fetchSentences,
    isPending: sentencesLoading,
    error: sentencesError,
  } = useGenerateSentences();

  // Default configuration
  const defaultConfig = useMemo(() => {
    return {
      difficulty: 1 as DifficultyLevel, // medium
      nikud: true,
      count: 3, // Fetch multiple sentences to reduce API calls
    };
  }, []);

  const finalConfig = useMemo(() => {
    return { ...defaultConfig, ...config };
  }, [defaultConfig, config]);

  const loading = sentencesLoading;
  const mutationError = sentencesError;

  // Helper to update state atomically
  const updateSentenceState = useCallback(
    (newExerciseId: string, newSentence: string, newWords: string[]) => {
      setSentenceState({
        exerciseId: newExerciseId,
        sentence: newSentence,
        words: newWords,
      });
    },
    [],
  );

  const fetchNewSentences = useCallback(async () => {
    if (pendingRef.current) return sentenceState;

    pendingRef.current = true;
    setError(null);

    // Fetch sentences
    const response = await fetchSentences({
      difficulty: finalConfig.difficulty,
      nikud: finalConfig.nikud,
      count: finalConfig.count,
      gameType: finalConfig.gameType,
    });


    setSentencePool(response);
    setCurrentIndex(0);

    if (response.length > 0) {
      const firstSentence = response[0] as SentenceItem;

      updateSentenceState(
        firstSentence.exerciseId,
        firstSentence.text,
        firstSentence.words,
      );

      pendingRef.current = false;
      return {
        sentence: firstSentence.text,
        words: firstSentence.words,
      };
    }

    // Clear state if no sentences
    updateSentenceState("", "", []);
    pendingRef.current = false;
    return { sentence: "", words: [] };
  }, [finalConfig, fetchSentences, updateSentenceState, sentenceState]);

  const getNextFromPool = useCallback(() => {
    if (sentencePool.length === 0) return { sentence: "", words: [] };

    const nextIndex = currentIndex + 1;

    // Check if we've reached the end of all sentences
    if (nextIndex >= sentencePool.length) {
      return { sentence: "", words: [] }; // Game over - no more sentences
    }

    setCurrentIndex(nextIndex);
    const nextItem = sentencePool[nextIndex];
    updateSentenceState(nextItem.exerciseId, nextItem.text, nextItem.words);
    return { sentence: nextItem.text, words: nextItem.words };
  }, [sentencePool, currentIndex, updateSentenceState]);

  const fetchSentence = useCallback(async () => {
    // If we have sentences in the pool and haven't reached the end, use next from pool
    if (sentencePool.length > 0 && currentIndex < sentencePool.length - 1) {
      const result = getNextFromPool();
      return result; // Return both sentence and words
    }

    // If we're at the last sentence, return empty to signal game over
    if (sentencePool.length > 0 && currentIndex >= sentencePool.length - 1) {
      return { sentence: "", words: [] };
    }

    // Otherwise, fetch new sentences from API
    const result = await fetchNewSentences();
    return result; // Return both sentence and words
  }, [sentencePool.length, currentIndex, getNextFromPool, fetchNewSentences]);

  const initOnce = useCallback(async () => {
    if (didInitRef.current) return;
    didInitRef.current = true;
    await fetchNewSentences();
  }, [fetchNewSentences]);

  // Reset function for restarting the game
  const resetGame = useCallback(() => {
    setCurrentIndex(0);
    setSentencePool([]);
    updateSentenceState(" ", "", []);
    didInitRef.current = false;
    pendingRef.current = false;
    setError(null);
  }, [updateSentenceState]);

  // Combine mutation error with any local errors
  const combinedError = error || (mutationError?.message ?? null);

  return {
    attemptId: sentenceState.exerciseId,
    sentence: sentenceState.sentence,
    words: sentenceState.words,
    loading,
    error: combinedError,
    fetchSentence,
    initOnce,
    resetGame,
    currentDifficulty: finalConfig.difficulty,
    hasNikud: finalConfig.nikud,
    sentenceCount: sentencePool.length,
    currentSentenceIndex: currentIndex,
  };
};
