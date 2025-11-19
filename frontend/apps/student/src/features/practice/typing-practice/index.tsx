import { useTranslation } from "react-i18next";
import { useState, useEffect, useCallback } from "react";
import { CircularProgress } from "@mui/material";
import { useStyles } from "./style";
import { FeedbackDisplay, AudioControls, RetryMode } from "./components";
import { useTypingPractice } from "./hooks";
import {
  GameConfig,
  GameSetupPanel,
  GameConfigModal,
  GameOverModal,
  GameSettings,
} from "@ui-components";
import {
  ContextAwareChat,
  useTypingPracticeContext,
} from "@ui-components/ContextAwareChat";
import { getDifficultyLabel } from "@student/features";
import { useGameConfig } from "@student/hooks";

interface RetryData {
  exerciseId: string;
  correctAnswer: string[];
  mistakes: Array<{
    wrongAnswer: string[];
    accuracy: number;
  }>;
  difficulty: number;
}

interface TypingPracticeProps {
  retryData?: RetryData;
}

const TypingPracticeMain = () => {
  const { t, i18n } = useTranslation();
  const classes = useStyles();
  const {
    config: savedConfig,
    isLoading: configLoading,
    updateConfig,
  } = useGameConfig("TypingPractice");
  const [gameOverModalOpen, setGameOverModalOpen] = useState(false);
  const [gameConfig, setGameConfig] = useState<GameConfig | null>(null);
  const [gameStarted, setGameStarted] = useState(false);
  const [configModalOpen, setConfigModalOpen] = useState(false);

  const isHebrew = i18n.language === "he" || i18n.language === "heb";

  const {
    exerciseState,
    currentExercise,
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
  } = useTypingPractice(gameConfig || undefined);

  useEffect(
    function initializeGameConfig() {
      if (gameConfig || configLoading) return;

      if (savedConfig) {
        setGameConfig(savedConfig);
        if (configModalOpen) {
          setConfigModalOpen(false);
        }
      } else {
        setConfigModalOpen(true);
      }
    },
    [gameConfig, configLoading, savedConfig, configModalOpen],
  );

  useEffect(() => {
    if (gameConfig && !gameStarted) {
      initOnce();
      setGameStarted(true);
    }
  }, [gameConfig, gameStarted, initOnce]);

  const handleConfigConfirm = useCallback(
    (config: GameConfig) => {
      setGameConfig(config);
      updateConfig(config);
      setConfigModalOpen(false);
      setGameStarted(false);
      resetGame();
    },
    [resetGame, updateConfig],
  );

  const handleConfigChange = useCallback(() => {
    setConfigModalOpen(true);
  }, []);

  const handleGameOverPlayAgain = useCallback(() => {
    setGameOverModalOpen(false);
    resetGame();
    setGameStarted(false);

    setTimeout(() => {
      setGameStarted(true);
      initOnce();
    }, 100);
  }, [resetGame, initOnce]);

  const handleGameOverChangeSettings = useCallback(() => {
    setGameOverModalOpen(false);
    setConfigModalOpen(true);
  }, []);

  const handleNextExerciseClick = useCallback(async () => {
    const result = await handleNextExercise();
    if (result.gameCompleted) {
      setGameOverModalOpen(true);
    }
  }, [handleNextExercise]);

  const pageContext = useTypingPracticeContext({
    currentExercise: currentSentenceIndex + 1,
    totalExercises: sentenceCount,
    difficulty: gameConfig?.difficulty.toString(),
    phraseToSpeak: currentExercise?.hebrewText,
    userAttempt: exerciseState.userInput,
    correctAnswer: exerciseState.feedbackResult?.expectedText,
    additionalContext: {
      phase: exerciseState.phase,
      correctCount: correctSentencesCount,
      isCorrect: exerciseState.feedbackResult?.isCorrect,
    },
  });

  const renderExerciseArea = () => (
    <div className={classes.exerciseArea} data-testid="typing-exercise-area">
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
              onKeyDown={(e) => e.key === "Enter" && handleSubmitAnswer()}
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
            onNextExercise={handleNextExerciseClick}
          />
        )}
      </div>
    </div>
  );

  if (configLoading) {
    return (
      <div className={classes.pageWrapper}>
        <div className={`${classes.container} ${classes.loadingContainer}`}>
          <CircularProgress />
        </div>
      </div>
    );
  }

  if (!gameStarted || !gameConfig) {
    return (
      <GameSetupPanel
        configModalOpen={configModalOpen}
        setConfigModalOpen={setConfigModalOpen}
        handleConfigConfirm={handleConfigConfirm}
        getDifficultyLabel={getDifficultyLabel}
      />
    );
  }

  return (
    <div className={classes.pageWrapper}>
      <div className={classes.container}>
        <div className={classes.content}>
          {exerciseState.error && (
            <div className={classes.errorContainer}>{exerciseState.error}</div>
          )}
          {renderExerciseArea()}
        </div>
        <div className={classes.gameSettingsWrapper}>
          <div
            className={`${classes.gameSettings} ${exerciseState.phase === "feedback" && classes.feedbackMode}`}
          >
            <GameSettings
              gameConfig={gameConfig}
              currentSentenceIndex={currentSentenceIndex}
              sentenceCount={sentenceCount}
              isHebrew={isHebrew}
              handleConfigChange={handleConfigChange}
              getDifficultyLabel={getDifficultyLabel}
            />
          </div>
        </div>

        {exerciseState.isLoading && (
          <div className={classes.loadingOverlay}>
            <CircularProgress />
          </div>
        )}

        <GameConfigModal
          open={configModalOpen}
          onClose={() => setConfigModalOpen(false)}
          onConfirm={handleConfigConfirm}
          getDifficultyLevelLabel={getDifficultyLabel}
          initialConfig={gameConfig || undefined}
        />

        <GameOverModal
          open={gameOverModalOpen}
          onPlayAgain={handleGameOverPlayAgain}
          onChangeSettings={handleGameOverChangeSettings}
          correctSentences={correctSentencesCount}
          totalSentences={sentenceCount}
        />
      </div>

      <ContextAwareChat pageContext={pageContext} />
    </div>
  );
};

export const TypingPractice = ({ retryData }: TypingPracticeProps) => {
  if (retryData) {
    return <RetryMode retryData={retryData} />;
  }
  return <TypingPracticeMain />;
};
