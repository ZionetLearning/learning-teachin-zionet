import { useState, useCallback, useRef, useMemo } from "react";
import { useGenerateSplitSentences } from "@student/api";
import { DifficultyLevel, GameType } from "@student/types";
import { SplitSentenceItem } from "@app-providers";

export interface UseWordOrderSentenceConfig {
  difficulty?: DifficultyLevel;
  nikud?: boolean;
  count?: number;
}

interface SentenceState {
  exerciseId: string;
  sentence: string;
  words: string[];
}

export const useWordOrderSentence = (
  config: UseWordOrderSentenceConfig = {},
) => {
  const [sentenceState, setSentenceState] = useState<SentenceState>({
    exerciseId: "",
    sentence: "",
    words: [],
  });

  const [sentencePool, setSentencePool] = useState<SplitSentenceItem[]>([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [error, setError] = useState<string | null>(null);

  const pendingRef = useRef(false);
  const didInitRef = useRef(false);

  const {
    mutateAsync: fetchSplitSentences,
    isPending: sentencesLoading,
    error: sentencesError,
  } = useGenerateSplitSentences();

  const defaultConfig = useMemo(() => {
    return {
      difficulty: 1 as DifficultyLevel,
      nikud: true,
      count: 3,
    };
  }, []);

  const finalConfig = useMemo(() => {
    return { ...defaultConfig, ...config };
  }, [defaultConfig, config]);

  const loading = sentencesLoading;
  const mutationError = sentencesError;

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

    const response = await fetchSplitSentences({
      difficulty: finalConfig.difficulty,
      nikud: finalConfig.nikud,
      count: finalConfig.count,
      gameType: GameType.WordOrderGame,
    });


    setSentencePool(response);
    setCurrentIndex(0);

    if (response.length > 0) {
      const firstSentence = response[0] as SplitSentenceItem;

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

    updateSentenceState("", "", []);
    pendingRef.current = false;
    return { sentence: "", words: [] };
  }, [finalConfig, fetchSplitSentences, updateSentenceState]);

  const getNextFromPool = useCallback(() => {
    if (sentencePool.length === 0) return { sentence: "", words: [] };

    const nextIndex = currentIndex + 1;

    if (nextIndex >= sentencePool.length) {
      return { sentence: "", words: [] };
    }

    setCurrentIndex(nextIndex);
    const nextItem = sentencePool[nextIndex];
    updateSentenceState(nextItem.exerciseId, nextItem.text, nextItem.words);
    return { sentence: nextItem.text, words: nextItem.words };
  }, [sentencePool, currentIndex, updateSentenceState]);

  const fetchSentence = useCallback(async () => {
    if (sentencePool.length > 0 && currentIndex < sentencePool.length - 1) {
      const result = getNextFromPool();
      return result;
    }

    if (sentencePool.length > 0 && currentIndex >= sentencePool.length - 1) {
      return { sentence: "", words: [] };
    }

    const result = await fetchNewSentences();
    return result;
  }, [sentencePool.length, currentIndex, getNextFromPool, fetchNewSentences]);

  const initOnce = useCallback(async () => {
    if (didInitRef.current) return;
    didInitRef.current = true;
    await fetchNewSentences();
  }, [fetchNewSentences]);

  const resetGame = useCallback(() => {
    setCurrentIndex(0);
    setSentencePool([]);
    updateSentenceState("", "", []);
    didInitRef.current = false;
    pendingRef.current = false;
    setError(null);
  }, [updateSentenceState]);

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
