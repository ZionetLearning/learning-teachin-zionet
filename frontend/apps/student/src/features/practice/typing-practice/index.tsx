import { useTranslation } from "react-i18next";
import { useStyles } from "./style";
import type { DifficultyLevel } from "./types";
import { LevelSelection, FeedbackDisplay, AudioControls } from "./components";
import { useTypingPractice } from "./hooks";

export const TypingPractice = () => {
  const { t } = useTranslation();
  const classes = useStyles();

  const {
    exerciseState,
    handleLevelSelect,
    handleBackToLevelSelection,
    handlePlayAudio,
    handleReplayAudio,
    handleInputChange,
    handleSubmitAnswer,
    handleTryAgain,
    handleNextExercise,
  } = useTypingPractice();

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
    <div className={classes.exerciseArea} data-testid="typing-exercise-area">
      <div className={classes.exerciseHeader}>
        <h3 className={classes.exerciseTitle}>
          {t("pages.typingPractice.hebrewTypingPractice")}
        </h3>
        <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
          {exerciseState.selectedLevel && (
            <span
              className={`${classes.levelBadge} ${getLevelBadgeClass(exerciseState.selectedLevel)}`}
              data-testid="typing-selected-level"
            >
              {exerciseState.selectedLevel}
            </span>
          )}
          <button
            className={classes.backButton}
            onClick={handleBackToLevelSelection}
            data-testid="typing-change-level"
          >
            {t("pages.typingPractice.changeLevel")}
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
          <div
            className={classes.typingInput}
            data-testid="typing-input-wrapper"
          >
            <input
              type="text"
              className={classes.typingInputField}
              value={exerciseState.userInput}
              onChange={handleInputChange}
              placeholder={t("pages.typingPractice.typeHereWhatYouHeard")}
              autoFocus
              data-testid="typing-input"
            />
            <button
              className={classes.typingSubmitButton}
              onClick={handleSubmitAnswer}
              disabled={!exerciseState.userInput.trim()}
              data-testid="typing-submit"
            >
              {t("pages.typingPractice.submitAnswer")}
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
        <h1 className={classes.title}>
          {t("pages.typingPractice.hebrewTypingPractice")}
        </h1>
        <p className={classes.subtitle}>
          {t("pages.typingPractice.listenToHebrewAudio")}
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
