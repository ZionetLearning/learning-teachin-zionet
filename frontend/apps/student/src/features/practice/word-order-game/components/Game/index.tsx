import { useState, useEffect, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { useQueryClient } from "@tanstack/react-query";

import { useAvatarSpeech, useHebrewSentence } from "@student/hooks";
import { ChosenWordsArea, WordsBank, ActionButtons, Speaker } from "../";
import {
  GameConfig,
  GameConfigModal,
  GameOverModal,
  GameSettings,
  GameSetupPanel,
  RetryResultModal,
} from "@ui-components";
import {
  MistakeChatPopup,
  WrongAnswerDisplay,
  ContextAwareChat,
} from "@student/components";
import { useWordOrderContext } from "@student/components/ContextAwareChat/hooks";
import { getDifficultyLabel } from "@student/features";
import { useAuth } from "@app-providers";
import { useSubmitGameAttempt } from "@student/api";
import { useStyles } from "./style";
import { toast } from "react-toastify";

interface RetryData {
  correctAnswer: string[];
  attemptId: string;
  wrongAnswers: string[][];
  difficulty: number;
}

interface GameProps {
  retryData?: RetryData;
}

export const Game = ({ retryData }: GameProps) => {
  const { user } = useAuth();
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const classes = useStyles();

  const studentId = user?.userId ?? "";
  const { mutateAsync: submitAttempt } = useSubmitGameAttempt();

  const [chosen, setChosen] = useState<string[]>([]);
  const [shuffledSentence, setShuffledSentence] = useState<string[]>([]);
  const [configModalOpen, setConfigModalOpen] = useState(false);
  const [gameOverModalOpen, setGameOverModalOpen] = useState(false);
  const [gameConfig, setGameConfig] = useState<GameConfig | null>(null);
  const [gameStarted, setGameStarted] = useState(false);
  const [correctSentencesCount, setCorrectSentencesCount] = useState<number>(0);
  const [hasCheckedThisSentence, setHasCheckedThisSentence] = useState(false);
  const [lastCheckWasIncorrect, setLastCheckWasIncorrect] = useState(false);
  const [mistakeChatOpen, setMistakeChatOpen] = useState(false);
  const [currentAttemptId, setCurrentAttemptId] = useState<string>("");
  const [isRetryMode] = useState(!!retryData);
  const [retryAttemptId] = useState(retryData?.attemptId || "");
  const [retryResultModalOpen, setRetryResultModalOpen] = useState(false);
  const [retryResult, setRetryResult] = useState<boolean | null>(null);

  const isHebrew = i18n.language === "he";

  const {
    attemptId,
    sentence,
    words,
    loading,
    error,
    fetchSentence,
    initOnce,
    resetGame,
    sentenceCount,
    currentSentenceIndex,
  } = useHebrewSentence(gameConfig || undefined);

  const { speak, stop, isLoading: speechLoading } = useAvatarSpeech({});

  useEffect(
    function showConfigModalOnFirstLoad() {
      if (!gameStarted && !gameConfig && !isRetryMode) {
        setConfigModalOpen(true);
      }
    },
    [gameStarted, gameConfig, isRetryMode],
  );

  const shuffleDistinct = useCallback((words: string[]) => {
    if (words.length < 2) return [...words];

    const shuffled = [...words];

    for (let i = shuffled.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]];
    }

    if (shuffled.join(" ") === words.join(" ") && words.length > 1) {
      [shuffled[0], shuffled[1]] = [shuffled[1], shuffled[0]];
    }

    return shuffled;
  }, []);

  useEffect(
    function initializeGameOrRetryMode() {
      if (isRetryMode && retryData && !gameStarted) {
        const shuffledWords = shuffleDistinct(retryData.correctAnswer);

        setChosen([]);
        setShuffledSentence(shuffledWords);
        setGameStarted(true);
        setHasCheckedThisSentence(false);

        const retryConfig = {
          difficulty: retryData.difficulty as 0 | 1 | 2,
          nikud: true,
          count: 1,
        };
        setGameConfig(retryConfig);
      } else if (gameConfig && !gameStarted && !isRetryMode) {
        initOnce();
        setGameStarted(true);
      }
    },
    [
      gameConfig,
      gameStarted,
      initOnce,
      isRetryMode,
      retryData,
      shuffleDistinct,
    ],
  );

  // Handle new sentences
  useEffect(
    function handleNewSentenceData() {
      if (!sentence || words.length === 0) {
        return;
      }
      setChosen([]);
      setShuffledSentence(shuffleDistinct(words));
      setHasCheckedThisSentence(false);
    },
    [sentence, words, shuffleDistinct],
  );

  const handleConfigConfirm = (config: GameConfig) => {
    setGameConfig(config);
    setConfigModalOpen(false);
    // Reset game state when config changes
    setChosen([]);
    setShuffledSentence([]);
    setCorrectSentencesCount(0);
    setGameStarted(false);
    setHasCheckedThisSentence(false);
    setLastCheckWasIncorrect(false);
    setCurrentAttemptId("");
    setMistakeChatOpen(false);
    resetGame();
  };

  const handleConfigChange = () => {
    setConfigModalOpen(true);
  };

  const handlePlay = () => {
    let sentenceToSpeak = sentence;

    if (isRetryMode && retryData) {
      sentenceToSpeak = retryData.correctAnswer.join(" ");
    }

    if (!sentenceToSpeak || sentenceToSpeak.trim() === "" || speechLoading) {
      return;
    }
    speak(sentenceToSpeak);
  };

  const handleNextClick = useCallback(async () => {
    stop();

    const result = await fetchSentence();

    // Game over?
    if (!result || !result.sentence) {
      setGameOverModalOpen(true);
      return;
    }

    // Prepare next sentence
    setChosen([]);
    if (result?.words?.length > 0) {
      setShuffledSentence(shuffleDistinct(result.words));
    }
    setHasCheckedThisSentence(false);
    setLastCheckWasIncorrect(false);
    setCurrentAttemptId("");
    setMistakeChatOpen(false);
  }, [stop, fetchSentence, shuffleDistinct]);

  const handleGameOverPlayAgain = () => {
    setGameOverModalOpen(false);
    resetGame();
    // Reset component state
    setChosen([]);
    setShuffledSentence([]);
    setGameStarted(false);
    setCorrectSentencesCount(0);
    setHasCheckedThisSentence(false);
    setLastCheckWasIncorrect(false);
    setCurrentAttemptId("");
    setMistakeChatOpen(false);

    // Restart the game with same config
    setTimeout(() => {
      setGameStarted(true);
      initOnce();
    }, 100);
  };

  const handleGameOverChangeSettings = () => {
    setGameOverModalOpen(false);
    setConfigModalOpen(true);
  };

  const handleReset = () => {
    setChosen([]);

    const wordsToShuffle =
      isRetryMode && retryData ? retryData.correctAnswer : words;

    if (wordsToShuffle.length > 0) {
      setShuffledSentence(shuffleDistinct(wordsToShuffle));
    }

    setHasCheckedThisSentence(false);
    setLastCheckWasIncorrect(false);
    setCurrentAttemptId("");
  };

  const handleRetryAgain = () => {
    setChosen([]);
    setHasCheckedThisSentence(false);
    setRetryResultModalOpen(false);

    if (retryData) {
      const shuffledWords = shuffleDistinct(retryData.correctAnswer);
      setShuffledSentence(shuffledWords);
    }
  };

  const handleBackToMistakes = () => {
    navigate("/practice-mistakes");
  };

  const handleModalBackToMistakes = () => {
    setRetryResultModalOpen(false);
    handleBackToMistakes();
  };

  const handleChooseWord = (word: string) => {
    setShuffledSentence((prev) => {
      const index = prev.indexOf(word);
      if (index > -1) {
        const newArray = [...prev];
        newArray.splice(index, 1); // Remove only the first occurrence
        return newArray;
      }
      return prev;
    });
    setChosen((prev) => [...prev, word]);
    setHasCheckedThisSentence(false); // changing answer invalidates prior check
    setLastCheckWasIncorrect(false);
  };

  const handleUnchooseWord = (index: number, word: string) => {
    setChosen((prev) => prev.filter((_, i) => i !== index));
    setShuffledSentence((prev) => [word, ...prev]);
    setHasCheckedThisSentence(false); // changing answer invalidates prior check
    setLastCheckWasIncorrect(false);
  };

  const handleCheck = useCallback(async () => {
    const currentAttemptId = isRetryMode ? retryAttemptId : attemptId;

    const res = await submitAttempt({
      attemptId: currentAttemptId,
      studentId,
      givenAnswer: chosen,
    });

    const isServerCorrect = res.status === "Success";

    if (isRetryMode) {
      setRetryResult(isServerCorrect);
      setRetryResultModalOpen(true);

      if (isServerCorrect) {
        queryClient.invalidateQueries({
          queryKey: ["gamesMistakes", { studentId }],
        });
      }
    } else {
      if (isServerCorrect) {
        setCorrectSentencesCount((c) => c + 1);
        toast.success(t("pages.wordOrderGame.correct"));
        setLastCheckWasIncorrect(false);
      } else {
        toast.error(t("pages.wordOrderGame.incorrect"));
        setLastCheckWasIncorrect(true);
        setCurrentAttemptId(attemptId);
      }
    }

    setHasCheckedThisSentence(true);

    return isServerCorrect;
  }, [
    submitAttempt,
    attemptId,
    retryAttemptId,
    isRetryMode,
    studentId,
    chosen,
    t,
    queryClient,
  ]);

  const handleExplainMistake = useCallback(() => {
    if (currentAttemptId) {
      setMistakeChatOpen(true);
    }
  }, [currentAttemptId]);

  const handleCloseMistakeChat = useCallback(() => {
    setMistakeChatOpen(false);
  }, []);

  const pageContext = useWordOrderContext({
    currentExercise: currentSentenceIndex + 1,
    totalExercises: sentenceCount,
    difficulty: gameConfig?.difficulty?.toString(),
    targetSentence: isRetryMode ? retryData?.correctAnswer.join(" ") : sentence,
    availableWords: shuffledSentence,
    userAnswer: chosen,
    additionalContext: {
      isRetryMode,
      correctCount: correctSentencesCount,
      hasChecked: hasCheckedThisSentence,
      chosenWordsCount: chosen.length,
    },
  });

  // Show welcome screen if game hasn't started yet
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
    <>
      <div className={classes.gameContainer}>
        <GameSettings
          gameConfig={gameConfig}
          currentSentenceIndex={currentSentenceIndex}
          sentenceCount={sentenceCount}
          isHebrew={isHebrew}
          handleConfigChange={handleConfigChange}
          getDifficultyLabel={getDifficultyLabel}
        />

        <div className={classes.gameLogic}>
          <div className={classes.speakersContainer}>
            <Speaker
              onClick={() => handlePlay()}
              disabled={
                isRetryMode
                  ? !retryData?.correctAnswer?.length || speechLoading
                  : !sentence || sentence.trim() === "" || speechLoading
              }
            />
            <ActionButtons
              loading={loading}
              handleNextClick={handleNextClick}
              handleCheck={handleCheck}
              handleReset={handleReset}
              showNext={hasCheckedThisSentence}
              showExplainMistake={
                hasCheckedThisSentence && lastCheckWasIncorrect
              }
              handleExplainMistake={handleExplainMistake}
            />
          </div>

          <ChosenWordsArea
            chosenWords={chosen}
            handleUnchooseWord={handleUnchooseWord}
          />

          <WrongAnswerDisplay
            wrongAnswer={
              isRetryMode && retryData && retryData.wrongAnswers.length > 0
                ? retryData.wrongAnswers[retryData.wrongAnswers.length - 1]
                : []
            }
            show={isRetryMode && retryData && retryData.wrongAnswers.length > 0}
          />

          <WordsBank
            loading={loading}
            error={error}
            shuffledSentence={shuffledSentence}
            handleChooseWord={handleChooseWord}
          />
        </div>
      </div>

      <ContextAwareChat pageContext={pageContext} hasSettings />

      {/* Configuration Modal */}
      <GameConfigModal
        open={configModalOpen}
        onClose={() => setConfigModalOpen(false)}
        onConfirm={handleConfigConfirm}
        getDifficultyLevelLabel={getDifficultyLabel}
        initialConfig={gameConfig || undefined}
      />

      {/* Game Over Modal */}
      <GameOverModal
        open={gameOverModalOpen}
        onPlayAgain={handleGameOverPlayAgain}
        onChangeSettings={handleGameOverChangeSettings}
        correctSentences={correctSentencesCount}
        totalSentences={sentenceCount}
      />

      {/* Mistake Chat Popup */}
      <MistakeChatPopup
        open={mistakeChatOpen}
        onClose={handleCloseMistakeChat}
        attemptId={currentAttemptId}
        gameType="word-order"
        title={t("pages.wordOrderGame.explainMistake")}
      />

      {/* Retry Result Modal */}
      <RetryResultModal
        open={retryResultModalOpen}
        isCorrect={retryResult === true}
        onRetryAgain={handleRetryAgain}
        onBackToMistakes={handleModalBackToMistakes}
      />
    </>
  );
};
