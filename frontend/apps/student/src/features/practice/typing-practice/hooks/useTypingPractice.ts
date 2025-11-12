import { useEffect, useState } from "react";
import type { ExerciseState, DifficultyLevel } from "../types";
import { compareTexts } from "../utils";
import { useAvatarSpeech, useHebrewSentence } from "@student/hooks";
import { CypressWindow, GameType } from "@student/types";
import { GameConfig } from "@ui-components";
import { useSubmitGameAttempt } from "@student/api";

const mapDifficultyToDifficultyLevel = (
  difficulty: 0 | 1 | 2,
): DifficultyLevel => {
  switch (difficulty) {
    case 0:
      return "easy";
    case 1:
      return "medium";
    case 2:
      return "hard";
    default:
      return "easy";
  }
};

export const useTypingPractice = (gameConfig?: GameConfig) => {
  const [exerciseState, setExerciseState] = useState<ExerciseState>({
    phase: "level-selection",
    selectedLevel: null,
    isLoading: false,
    error: null,
    audioState: {
      isPlaying: false,
      hasPlayed: false,
      error: null,
    },
    userInput: "",
    feedbackResult: null,
  });

  const [correctSentencesCount, setCorrectSentencesCount] = useState<number>(0);
  const { mutateAsync: submitAttempt } = useSubmitGameAttempt();

  const {
    attemptId: exerciseId,
    sentence,
    loading,
    error: sentenceError,
    fetchSentence,
    initOnce,
    resetGame: resetSentenceGameHook,
    sentenceCount,
    currentSentenceIndex,
  } = useHebrewSentence(gameConfig ? { ...gameConfig, gameType: GameType.TypingPractice } : undefined);

  const { speak, stop, isPlaying, error } = useAvatarSpeech({
    volume: 1,
    onAudioStart: () => {
      setExerciseState((prev) => ({
        ...prev,
        phase: "playing",
        audioState: {
          ...prev.audioState,
          isPlaying: true,
          error: null,
        },
      }));
      // In Cypress (or non-audio) environments the onAudioEnd callback might never fire.
      try {
        if (
          typeof window !== "undefined" &&
          (window as CypressWindow).Cypress
        ) {
          setTimeout(() => {
            setExerciseState((prev) => {
              if (prev.phase !== "playing") return prev;
              return {
                ...prev,
                phase: "typing",
                audioState: {
                  ...prev.audioState,
                  isPlaying: false,
                  hasPlayed: true,
                  error: null,
                },
              };
            });
          }, 300);
        }
      } catch {
        /* ignore */
      }
    },
    onAudioEnd: () => {
      setExerciseState((prev) => ({
        ...prev,
        phase: "typing",
        audioState: {
          ...prev.audioState,
          isPlaying: false,
          hasPlayed: true,
          error: null,
        },
      }));
    },
  });

  useEffect(
    function handleError() {
      if (error) {
        setExerciseState((prev) => ({
          ...prev,
          phase: prev.phase === "playing" ? "ready" : prev.phase,
          audioState: {
            ...prev.audioState,
            isPlaying: false,
            error: error instanceof Error ? error.message : "TTS error",
          },
        }));
      }
    },
    [error],
  );

  // Handle sentence error
  useEffect(() => {
    if (sentenceError) {
      setExerciseState((prev) => ({
        ...prev,
        error: sentenceError,
      }));
    }
  }, [sentenceError]);

  // Initialize game when config is provided
  useEffect(() => {
    if (gameConfig && gameConfig.difficulty !== undefined) {
      const difficultyLevel = mapDifficultyToDifficultyLevel(
        gameConfig.difficulty,
      );
      setExerciseState((prev) => ({
        ...prev,
        selectedLevel: difficultyLevel,
        phase: "ready",
        isLoading: false,
        error: null,
        audioState: {
          isPlaying: false,
          hasPlayed: false,
          error: null,
        },
        userInput: "",
        feedbackResult: null,
      }));
    }
  }, [gameConfig]);

  useEffect(() => {
    setExerciseState((prev) => ({
      ...prev,
      isLoading: loading,
    }));
  }, [loading]);

  const resetGame = () => {
    setCorrectSentencesCount(0);
    resetSentenceGameHook();
    setExerciseState((prev) => {
      const hasConfig = !!gameConfig && gameConfig.difficulty !== undefined;
      return {
        ...prev,
        phase: hasConfig ? "ready" : "level-selection",
        selectedLevel: hasConfig
          ? mapDifficultyToDifficultyLevel(gameConfig!.difficulty)
          : null,
        isLoading: false,
        error: null,
        audioState: {
          isPlaying: false,
          hasPlayed: false,
          error: null,
        },
        userInput: "",
        feedbackResult: null,
      };
    });
  };

  const handlePlayAudio = async (): Promise<void> => {
    if (!sentence) {
      return;
    }

    if (isPlaying) {
      stop();
      return;
    }

    setExerciseState((prev) => ({
      ...prev,
      error: null,
      audioState: {
        ...prev.audioState,
        error: null,
      },
    }));
    speak(sentence);
  };

  const handleReplayAudio = async () => {
    if (!sentence) return;

    if (isPlaying) {
      stop();
      return;
    }

    setExerciseState((prev) => ({
      ...prev,
      error: null,
      audioState: {
        ...prev.audioState,
        error: null,
      },
    }));
    speak(sentence);
  };

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setExerciseState((prev) => ({
      ...prev,
      userInput: event.target.value,
    }));
  };

  const handleSubmitAnswer = async () => {
    if (!sentence || !exerciseState.userInput.trim() || !exerciseId) return;

    const feedbackResult = compareTexts(exerciseState.userInput, sentence);

    // Submit attempt to backend
    try {
      const res = await submitAttempt({
        exerciseId: exerciseId,
        givenAnswer: [exerciseState.userInput], // Send the full sentence as first element
      });

      // Update feedback with server accuracy
      const updatedFeedback = {
        ...feedbackResult,
        accuracy: res.accuracy,
      };

      setExerciseState((prev) => ({
        ...prev,
        phase: "feedback",
        feedbackResult: updatedFeedback,
      }));

      // Track correct answers based on server response
      if (res.status === "Success") {
        setCorrectSentencesCount((prev) => prev + 1);
      }
    } catch (error) {
      console.error("Failed to submit typing practice attempt:", error);
      // Still show local feedback even if submission fails
      setExerciseState((prev) => ({
        ...prev,
        phase: "feedback",
        feedbackResult,
      }));

      if (feedbackResult.isCorrect) {
        setCorrectSentencesCount((prev) => prev + 1);
      }
    }
  };

  const handleTryAgain = () => {
    setExerciseState((prev) => ({
      ...prev,
      phase: "typing",
      userInput: "",
      feedbackResult: null,
    }));
  };

  const handleNextExercise = async (): Promise<{ gameCompleted: boolean }> => {
    if (!exerciseState.selectedLevel) return { gameCompleted: false };

    try {
      if (isPlaying) stop();
      const result = await fetchSentence();

      // Check if game is completed (no more sentences)
      if (!result || !result.sentence) {
        return { gameCompleted: true };
      }

      setExerciseState((prev) => ({
        ...prev,
        phase: "ready",
        userInput: "",
        feedbackResult: null,
        audioState: {
          isPlaying: false,
          hasPlayed: false,
          error: null,
        },
      }));

      return { gameCompleted: false };
    } catch (error) {
      setExerciseState((prev) => ({
        ...prev,
        error:
          error instanceof Error
            ? error.message
            : "Failed to load next exercise",
      }));
      return { gameCompleted: false };
    }
  };

  return {
    exerciseState,
    currentExercise: sentence ? { hebrewText: sentence } : null,
    currentSentenceIndex,
    correctSentencesCount,
    sentenceCount,
    resetGame,
    initOnce,
    handlePlayAudio,
    handleReplayAudio,
    handleInputChange,
    handleSubmitAnswer,
    handleTryAgain,
    handleNextExercise,
  };
};
