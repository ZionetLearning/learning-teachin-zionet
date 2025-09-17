import { useState, useEffect, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { useStyles } from "./style";
import { Speaker } from "../Speaker";
import { useHebrewSentence } from "../../hooks";
import { useAvatarSpeech } from "@student/hooks";
import { DifficultyLevel } from "@student/types";
import {
  GameConfigModal,
  GameConfig,
  GameOverModal,
  WelcomeScreen,
  GameHeaderSettings,
  ChosenWordsArea,
  WordsBank,
  SideButtons,
} from "../";

export const Game = () => {
  const { t, i18n } = useTranslation();
  const classes = useStyles();
  const [chosen, setChosen] = useState<string[]>([]);
  const [shuffledSentence, setShuffledSentence] = useState<string[]>([]);
  const [isCorrect, setIsCorrect] = useState<boolean>(false);
  const [configModalOpen, setConfigModalOpen] = useState(false);
  const [gameOverModalOpen, setGameOverModalOpen] = useState(false);
  const [gameConfig, setGameConfig] = useState<GameConfig | null>(null);
  const [gameStarted, setGameStarted] = useState(false);
  const [correctSentencesCount, setCorrectSentencesCount] = useState<number>(0);
  const isHebrew = i18n.language === "he" || i18n.language === "heb";

  const {
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

  // Show config modal on first load
  useEffect(() => {
    if (!gameStarted && !gameConfig) {
      setConfigModalOpen(true);
    }
  }, [gameStarted, gameConfig]);

  // Initialize game when config is set
  useEffect(() => {
    if (gameConfig && !gameStarted) {
      initOnce();
      setGameStarted(true);
    }
  }, [gameConfig, gameStarted, initOnce]);

  // Handle new sentences
  useEffect(() => {
    const handleNewSentence = () => {
      if (!sentence || words.length === 0) {
        return;
      }
      setChosen([]);
      setShuffledSentence(shuffleDistinct(words));
    };
    handleNewSentence();
  }, [sentence, words]);

  useEffect(() => {
    if (words.length > 0) {
      setIsCorrect(chosen.join(" ") === words.join(" "));
    }
  }, [words, chosen]);

  const handleConfigConfirm = (config: GameConfig) => {
    setGameConfig(config);
    setConfigModalOpen(false);
    // Reset game state when config changes
    setChosen([]);
    setShuffledSentence([]);
    setCorrectSentencesCount(0);
    setGameStarted(false);
    // Reset the hook's internal state
    resetGame();
  };

  const handleConfigChange = () => {
    setConfigModalOpen(true);
  };

  const handlePlay = () => {
    if (!sentence || sentence.trim() === "" || speechLoading) {
      return;
    }
    speak(sentence);
  };

  const handleNextClick = useCallback(async () => {
    stop();
    if (isCorrect) {
      setCorrectSentencesCount(correctSentencesCount + 1);
    }

    const result = await fetchSentence();

    // Check if we've completed all sentences
    if (!result || !result.sentence) {
      setGameOverModalOpen(true);
      return;
    }

    setChosen([]);
    if (result.words && result.words.length > 0) {
      setShuffledSentence(shuffleDistinct(result.words));
    }
  }, [stop, fetchSentence, isCorrect]);

  const handleGameOverPlayAgain = () => {
    setGameOverModalOpen(false);
    // Reset the hook's internal state
    resetGame();
    // Reset component state
    setChosen([]);
    setShuffledSentence([]);
    setGameStarted(false);
    setCorrectSentencesCount(0);
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
    if (!sentence) return;
    setChosen([]);

    if (words.length > 0) {
      setShuffledSentence(shuffleDistinct(words));
    }
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
  };

  const handleUnchooseWord = (index: number, word: string) => {
    setChosen((prev) => prev.filter((_, i) => i !== index));
    setShuffledSentence((prev) => [word, ...prev]);
  };

  const handleCheck = () => {
    alert(isCorrect ? "Correct!" : "Incorrect! Try again");
    return isCorrect;
  };

  const shuffleDistinct = (words: string[]) => {
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
  };

  const getDifficultyLabel = (difficulty: DifficultyLevel) => {
    switch (difficulty) {
      case 0:
        return t("pages.wordOrderGame.difficulty.easy");
      case 1:
        return t("pages.wordOrderGame.difficulty.medium");
      case 2:
        return t("pages.wordOrderGame.difficulty.hard");
      default:
        return t("pages.wordOrderGame.difficulty.medium");
    }
  };

  // Show welcome screen if game hasn't started yet
  if (!gameStarted || !gameConfig) {
    return (
      <WelcomeScreen
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
        {/* Game Header with Settings */}
        <GameHeaderSettings
          gameConfig={gameConfig}
          currentSentenceIndex={currentSentenceIndex}
          sentenceCount={sentenceCount}
          isHebrew={isHebrew}
          handleConfigChange={handleConfigChange}
          getDifficultyLabel={getDifficultyLabel}
        />

        <div className={classes.gameLogic}>
          <div className={classes.speakersContainer}>
            {speechLoading ? (
              <div>{t("pages.wordOrderGame.loading")}</div>
            ) : (
              <Speaker
                onClick={() => handlePlay()}
                disabled={!sentence || sentence.trim() === "" || speechLoading}
              />
            )}
          </div>

          <ChosenWordsArea
            chosenWords={chosen}
            handleUnchooseWord={handleUnchooseWord}
          />
          <WordsBank
            loading={loading}
            error={error}
            shuffledSentence={shuffledSentence}
            handleChooseWord={handleChooseWord}
          />
        </div>
        <SideButtons
          loading={loading}
          handleNextClick={handleNextClick}
          handleCheck={handleCheck}
          handleReset={handleReset}
        />
      </div>
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
    </>
  );
};
