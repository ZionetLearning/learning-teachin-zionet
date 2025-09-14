import { useState, useCallback, useRef } from "react";
import {
  useFetchSentences,
  useGenerateSplitSentences,
  SentenceItem,
  SplitSentenceItem,
} from "@student/api";

export type DifficultyLevel = 0 | 1 | 2; // 0=easy, 1=medium, 2=hard
export type SentenceMode = "regular" | "split";

export interface UseHebrewSentenceConfig {
  difficulty?: DifficultyLevel;
  nikud?: boolean;
  count?: number;
  mode?: SentenceMode;
}

export const useHebrewSentence = (config: UseHebrewSentenceConfig = {}) => {
  const [sentence, setSentence] = useState<string>("");
  const [words, setWords] = useState<string[]>([]);
  const [originalSentence, setOriginalSentence] = useState<string>("");
  const [sentencePool, setSentencePool] = useState<
    (SentenceItem | SplitSentenceItem)[]
  >([]);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [error, setError] = useState<string | null>(null);

  const pendingRef = useRef(false);
  const didInitRef = useRef(false);

  // Both API hooks
  const {
    mutateAsync: fetchRegularSentences,
    isPending: regularLoading,
    error: regularError,
  } = useFetchSentences();

  const {
    mutateAsync: fetchSplitSentences,
    isPending: splitLoading,
    error: splitError,
  } = useGenerateSplitSentences();

  // Default configuration
  const defaultConfig = {
    difficulty: 1 as DifficultyLevel, // medium
    nikud: true,
    count: 3, // Fetch multiple sentences to reduce API calls
    mode: "regular" as SentenceMode,
  };

  const finalConfig = { ...defaultConfig, ...config };
  const loading =
    finalConfig.mode === "regular" ? regularLoading : splitLoading;
  const mutationError =
    finalConfig.mode === "regular" ? regularError : splitError;

  const fetchNewSentences = useCallback(async () => {
    if (pendingRef.current) return { sentence, words };

    pendingRef.current = true;
    setError(null);

    try {
      if (finalConfig.mode === "split") {
        // Fetch split sentences
        const response = await fetchSplitSentences({
          difficulty: finalConfig.difficulty,
          nikud: finalConfig.nikud,
          count: finalConfig.count,
        });

        setSentencePool(response.sentences);
        setCurrentIndex(0);

        if (response.sentences.length > 0) {
          const firstSentence = response.sentences[0] as SplitSentenceItem;
          setSentence(firstSentence.original);
          setWords(firstSentence.words);
          setOriginalSentence(firstSentence.original);
          return {
            sentence: firstSentence.original,
            words: firstSentence.words,
          };
        }
      } else {
        // Fetch regular sentences
        const response = await fetchRegularSentences({
          difficulty: finalConfig.difficulty,
          nikud: finalConfig.nikud,
          count: finalConfig.count,
        });

        setSentencePool(response.sentences);
        setCurrentIndex(0);

        if (response.sentences.length > 0) {
          const firstSentence = response.sentences[0] as SentenceItem;
          setSentence(firstSentence.text);
          setWords(firstSentence.text.replace(/\./g, "").split(" "));
          setOriginalSentence(firstSentence.text);
          return {
            sentence: firstSentence.text,
            words: firstSentence.text.replace(/\./g, "").split(" "),
          };
        }
      }

      return { sentence: "", words: [] };
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "An unknown error occurred";
      setError(errorMessage);
      throw err;
    } finally {
      pendingRef.current = false;
    }
  }, [
    finalConfig,
    fetchRegularSentences,
    fetchSplitSentences,
    sentence,
    words,
  ]);

  const getNextFromPool = useCallback(() => {
    if (sentencePool.length === 0) return { sentence: "", words: [] };

    const nextIndex = (currentIndex + 1) % sentencePool.length;
    setCurrentIndex(nextIndex);

    const nextItem = sentencePool[nextIndex];

    if (finalConfig.mode === "split" && "words" in nextItem) {
      // Split sentence item
      const splitItem = nextItem as SplitSentenceItem;
      setSentence(splitItem.original);
      setWords(splitItem.words);
      setOriginalSentence(splitItem.original);
      return { sentence: splitItem.original, words: splitItem.words };
    } else if (finalConfig.mode === "regular" && "text" in nextItem) {
      // Regular sentence item
      const regularItem = nextItem as SentenceItem;
      const wordsArray = regularItem.text.replace(/\./g, "").split(" ");
      setSentence(regularItem.text);
      setWords(wordsArray);
      setOriginalSentence(regularItem.text);
      return { sentence: regularItem.text, words: wordsArray };
    }

    return { sentence: "", words: [] };
  }, [sentencePool, currentIndex, finalConfig.mode]);

  const fetchSentence = useCallback(async () => {
    // If we have sentences in the pool and haven't reached the end, use next from pool
    if (sentencePool.length > 0 && currentIndex < sentencePool.length - 1) {
      const result = getNextFromPool();
      return result.sentence;
    }

    // Otherwise, fetch new sentences from API
    const result = await fetchNewSentences();
    return result.sentence;
  }, [sentencePool.length, currentIndex, getNextFromPool, fetchNewSentences]);

  const initOnce = useCallback(async () => {
    if (didInitRef.current) return;
    didInitRef.current = true;
    await fetchNewSentences();
  }, [fetchNewSentences]);

  // Combine mutation error with any local errors
  const combinedError = error || (mutationError?.message ?? null);

  return {
    sentence,
    words,
    originalSentence,
    loading,
    error: combinedError,
    fetchSentence,
    initOnce,
    // Additional properties for debugging/info
    currentDifficulty: finalConfig.difficulty,
    hasNikud: finalConfig.nikud,
    mode: finalConfig.mode,
    sentenceCount: sentencePool.length,
    currentSentenceIndex: currentIndex,
  };
};
