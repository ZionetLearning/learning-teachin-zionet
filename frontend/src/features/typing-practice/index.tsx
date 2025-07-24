import { useState } from "react";
import { useStyles } from "./style";
import type { ExerciseState, DifficultyLevel, Exercise } from "./types";
import { getRandomExercise, compareTexts } from "./utils";
import { speakHebrew } from "../../services/azureTTS";
import { LevelSelection, FeedbackDisplay, AudioControls } from "./components";

export const TypingPractice = () => {
  const classes = useStyles();

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

  const getLevelBadgeClass = (level: DifficultyLevel) => {
    switch (level) {
      case "easy":
        return classes.levelBadgeEasy;
      case "medium":
        return classes.levelBadgeMedium;
      case "hard":
        return classes.levelBadgeHard;
      default:
        return classes.levelBadgeEasy;
    }
  };

  const renderExerciseArea = () => (
    <div className={classes.exerciseArea}>
      <div className={classes.exerciseHeader}>
        <h3 className={classes.exerciseTitle}>Hebrew Typing Practice</h3>
        <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
          {exerciseState.selectedLevel && (
            <span
              className={`${classes.levelBadge} ${getLevelBadgeClass(exerciseState.selectedLevel)}`}
            >
              {exerciseState.selectedLevel}
            </span>
          )}
          <button
            className={classes.backButton}
            onClick={handleBackToLevelSelection}
          >
            ← Change Level
          </button>
        </div>
      </div>

      <div className={classes.audioSection}>
        <AudioControls
          phase={
            exerciseState.phase as "ready" | "playing" | "typing" | "feedback"
          }
          audioState={exerciseState.audioState}
          onPlayAudio={handlePlayAudio}
          onReplayAudio={handleReplayAudio}
        />

        {exerciseState.phase === "typing" && (
          <div className={classes.typingInput}>
            <input
              type="text"
              className={classes.typingInputField}
              value={exerciseState.userInput}
              onChange={handleInputChange}
              placeholder="הקלד כאן את מה ששמעת..."
              autoFocus
            />
            <button
              className={classes.typingSubmitButton}
              onClick={handleSubmitAnswer}
              disabled={!exerciseState.userInput.trim()}
            >
              ✓ Submit Answer
            </button>
          </div>
        )}

        {exerciseState.phase === "feedback" && exerciseState.feedbackResult && (
          <FeedbackDisplay
            feedbackResult={exerciseState.feedbackResult}
            onTryAgain={handleTryAgain}
            onNextExercise={handleNextExercise}
            onChangeLevel={handleBackToLevelSelection}
          />
        )}
      </div>
    </div>
  );

  return (
    <div className={classes.container}>
      <div className={classes.header}>
        <h1 className={classes.title}>Hebrew Typing Practice</h1>
        <p className={classes.subtitle}>
          Listen to Hebrew audio and practice your typing skills
        </p>
      </div>

      <div className={classes.content}>
        {exerciseState.error && (
          <div className={classes.errorContainer}>{exerciseState.error}</div>
        )}

        {exerciseState.phase === "level-selection" ? (
          <LevelSelection
            onLevelSelect={handleLevelSelect}
            isLoading={exerciseState.isLoading}
          />
        ) : (
          renderExerciseArea()
        )}
      </div>

      {exerciseState.isLoading && (
        <div className={classes.loadingOverlay}>
          <div className={classes.loadingSpinner} />
        </div>
      )}
    </div>
  );
};
