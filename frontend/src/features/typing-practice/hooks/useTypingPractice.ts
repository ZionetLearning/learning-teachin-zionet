import { useState } from "react";
import type { ExerciseState, DifficultyLevel, Exercise } from "../types";
import { getRandomExercise, compareTexts } from "../utils";
import { speakHebrew } from "../../../services";

export const useTypingPractice = () => {
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

  const [currentExercise, setCurrentExercise] = useState<Exercise | null>(null);

  const handleLevelSelect = (level: DifficultyLevel) => {
    try {
      setExerciseState((prev) => ({ ...prev, isLoading: true, error: null }));

      const exercise = getRandomExercise(level);
      setCurrentExercise(exercise);

      setExerciseState({
        phase: "ready",
        selectedLevel: level,
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
    } catch (error) {
      setExerciseState((prev) => ({
        ...prev,
        isLoading: false,
        error:
          error instanceof Error ? error.message : "Failed to load exercise",
      }));
    }
  };

  const handleBackToLevelSelection = () => {
    setExerciseState({
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
    setCurrentExercise(null);
  };

  const handlePlayAudio = async () => {
    if (!currentExercise) return;

    setExerciseState((prev) => ({
      ...prev,
      phase: "playing",
      audioState: {
        ...prev.audioState,
        isPlaying: true,
        error: null,
      },
    }));

    try {
      await speakHebrew(currentExercise.hebrewText);

      setExerciseState((prev) => ({
        ...prev,
        phase: "typing",
        audioState: {
          ...prev.audioState,
          isPlaying: false,
          hasPlayed: true,
        },
      }));
    } catch (error) {
      const errorMessage =
        error instanceof Error ? error.message : "Failed to play audio";

      setExerciseState((prev) => ({
        ...prev,
        phase: "ready",
        audioState: {
          ...prev.audioState,
          isPlaying: false,
          error: errorMessage,
        },
      }));
    }
  };

  const handleReplayAudio = async () => {
    if (!currentExercise) return;

    setExerciseState((prev) => ({
      ...prev,
      phase: "playing",
      audioState: {
        ...prev.audioState,
        isPlaying: true,
        error: null,
      },
    }));

    try {
      await speakHebrew(currentExercise.hebrewText);

      setExerciseState((prev) => ({
        ...prev,
        phase: "typing",
        audioState: {
          ...prev.audioState,
          isPlaying: false,
        },
      }));
    } catch (error) {
      const errorMessage =
        error instanceof Error ? error.message : "Failed to replay audio";

      setExerciseState((prev) => ({
        ...prev,
        phase: "typing",
        audioState: {
          ...prev.audioState,
          isPlaying: false,
          error: errorMessage,
        },
      }));
    }
  };

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setExerciseState((prev) => ({
      ...prev,
      userInput: event.target.value,
    }));
  };

  const handleSubmitAnswer = () => {
    if (!currentExercise || !exerciseState.userInput.trim()) return;

    const feedbackResult = compareTexts(
      exerciseState.userInput,
      currentExercise.hebrewText,
    );

    setExerciseState((prev) => ({
      ...prev,
      phase: "feedback",
      feedbackResult,
    }));
  };

  const handleTryAgain = () => {
    setExerciseState((prev) => ({
      ...prev,
      phase: "typing",
      userInput: "",
      feedbackResult: null,
    }));
  };

  const handleNextExercise = () => {
    if (!exerciseState.selectedLevel) return;

    try {
      const exercise = getRandomExercise(exerciseState.selectedLevel);
      setCurrentExercise(exercise);

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
    } catch (error) {
      setExerciseState((prev) => ({
        ...prev,
        error:
          error instanceof Error
            ? error.message
            : "Failed to load next exercise",
      }));
    }
  };

  return {
    exerciseState,
    currentExercise,
    handleLevelSelect,
    handleBackToLevelSelection,
    handlePlayAudio,
    handleReplayAudio,
    handleInputChange,
    handleSubmitAnswer,
    handleTryAgain,
    handleNextExercise,
  };
};