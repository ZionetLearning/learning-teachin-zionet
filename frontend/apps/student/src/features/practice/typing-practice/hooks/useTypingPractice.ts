import { useCallback, useEffect, useState } from "react";
import type { ExerciseState, DifficultyLevel, Exercise } from "../types";
import { compareTexts } from "../utils";
import { useAvatarSpeech, useSignalR } from "@student/hooks";
import { usePostSentence } from "@student/api/sentence";

// === server event contracts ===
type EventType =
  | "SentenceGeneration"
  | "SplitSentenceGeneration";

type UserEvent<T> = {
  eventType: EventType;
  payload: T;
};

type SentenceItem = {
  text: string;
  difficulty: "easy" | "medium" | "hard" | string; // server sends string
  nikud: boolean;
};

type SentenceResponse = {
  sentences: SentenceItem[];
};

// === adapter: server -> Exercise (adjust if your Exercise needs more) ===
function toExercise(item: SentenceItem): Exercise {
  // Map the server sentence to the shape your UI expects.
  // At minimum, this hook uses `exercise.hebrewText` for audio & comparison.
  return {
    hebrewText: item.text,
  } as Exercise;
}

export const useTypingPractice = () => {
  const [exerciseState, setExerciseState] = useState<ExerciseState>({
    phase: "level-selection",
    selectedLevel: null,
    isLoading: false,
    error: null,
    audioState: { isPlaying: false, hasPlayed: false, error: null },
    userInput: "",
    feedbackResult: null,
  });

  const [currentExercise, setCurrentExercise] = useState<Exercise | null>(null);

  // --- audio (unchanged behavior) ---
  const { speak, stop, isPlaying, error: ttsError } = useAvatarSpeech({
    volume: 1,
    onAudioStart: () => {
      setExerciseState((prev) => ({
        ...prev,
        phase: "playing",
        audioState: { ...prev.audioState, isPlaying: true, error: null },
      }));
      // fail-safe: in test envs ensure we don't get stuck in "playing"
      setTimeout(() => {
        setExerciseState((prev) =>
          prev.phase === "playing" ? { ...prev, phase: "typing" } : prev,
        );
      }, 600);
    },
    onAudioEnd: () =>
      setExerciseState((prev) => ({
        ...prev,
        phase: "typing",
        audioState: { ...prev.audioState, isPlaying: false },
      })),
  });

  useEffect(() => {
    if (ttsError) {
      setExerciseState((prev) => ({
        ...prev,
        phase: prev.phase === "playing" ? "ready" : prev.phase,
        audioState: {
          ...prev.audioState,
          isPlaying: false,
          error: ttsError instanceof Error ? ttsError.message : "TTS error",
        },
      }));
    }
  }, [ttsError]);

  // --- HTTP mutation to request a sentence ---
  const { mutate: postSentence } = usePostSentence();

  // --- SignalR subscription: receive the sentence when ready ---
  const { subscribe } = useSignalR();

  const onReceiveEvent = useCallback(
    (evt: UserEvent<SentenceResponse>) => {
      if (!evt || evt.eventType !== "SentenceGeneration") return;

      const payload = evt.payload as SentenceResponse;
      const first = payload?.sentences?.[0];
      if (!first) {
        setExerciseState((prev) => ({
          ...prev,
          isLoading: false,
          error: "No sentences returned",
        }));
        return;
      }

      const exercise = toExercise(first);
      setCurrentExercise(exercise);

      setExerciseState((prev) => ({
        ...prev,
        phase: "ready",
        isLoading: false,
        error: null,
        audioState: { isPlaying: false, hasPlayed: false, error: null },
        userInput: "",
        feedbackResult: null,
      }));
    },
    [],
  );

  useEffect(() => {
    const unsubscribe = subscribe<UserEvent<SentenceResponse>>(
      "ReceiveEvent",
      onReceiveEvent,
    );
    return unsubscribe;
  }, [subscribe, onReceiveEvent]);

  // --- handlers ---
  const handleLevelSelect = useCallback(
    (level: DifficultyLevel) => {
      setExerciseState((prev) => ({
        ...prev,
        selectedLevel: level,
        isLoading: true,
        error: null,
        userInput: "",
        feedbackResult: null,
        audioState: { ...prev.audioState, isPlaying: false, hasPlayed: false, error: null },
      }));

      // request 1 sentence for the chosen level; adjust nikud as needed
      postSentence({ difficulty: level as DifficultyLevel, nikud: false, count: 1 });
    },
    [postSentence],
  );

  const handleBackToLevelSelection = useCallback(() => {
    if (isPlaying) stop();
    setCurrentExercise(null);
    setExerciseState((prev) => ({
      ...prev,
      phase: "level-selection",
      selectedLevel: null,
      isLoading: false,
      error: null,
      audioState: { isPlaying: false, hasPlayed: false, error: null },
      userInput: "",
      feedbackResult: null,
    }));
  }, [isPlaying, stop]);

  const handlePlayAudio = useCallback(() => {
    if (!currentExercise) return;
    if (isPlaying) {
      stop();
      return;
    }
    setExerciseState((prev) => ({
      ...prev,
      error: null,
      audioState: { ...prev.audioState, error: null },
    }));
    speak(currentExercise.hebrewText);
  }, [currentExercise, isPlaying, speak, stop]);

  const handleReplayAudio = useCallback(() => {
    if (!currentExercise) return;
    if (isPlaying) {
      stop();
      return;
    }
    setExerciseState((prev) => ({
      ...prev,
      error: null,
      audioState: { ...prev.audioState, error: null },
    }));
    speak(currentExercise.hebrewText);
  }, [currentExercise, isPlaying, speak, stop]);

  const handleInputChange = useCallback(
    (event: React.ChangeEvent<HTMLInputElement>) => {
      setExerciseState((prev) => ({ ...prev, userInput: event.target.value }));
    },
    [],
  );

  const handleSubmitAnswer = useCallback(() => {
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
  }, [currentExercise, exerciseState.userInput]);

  const handleTryAgain = useCallback(() => {
    setExerciseState((prev) => ({
      ...prev,
      phase: "typing",
      userInput: "",
      feedbackResult: null,
    }));
  }, []);

  const handleNextExercise = useCallback(() => {
    if (!exerciseState.selectedLevel) return;

    if (isPlaying) stop();

    // ask backend for another sentence; keep same level
    setExerciseState((prev) => ({
      ...prev,
      isLoading: true,
      error: null,
      userInput: "",
      feedbackResult: null,
      audioState: { isPlaying: false, hasPlayed: false, error: null },
    }));

    postSentence({
      difficulty: exerciseState.selectedLevel as DifficultyLevel,
      nikud: false,
      count: 1,
    });
  }, [exerciseState.selectedLevel, isPlaying, postSentence, stop]);

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
