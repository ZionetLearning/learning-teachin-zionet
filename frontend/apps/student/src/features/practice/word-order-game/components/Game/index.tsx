import { useState, useEffect, useMemo } from "react";
import { useTranslation } from "react-i18next";
import { useStyles } from "./style";
import { Speaker } from "../Speaker";
import { useHebrewSentence } from "../../hooks";
import { useAvatarSpeech } from "@student/hooks";
import { DifficultyLevel } from "@student/types";
import { GameConfigModal, GameConfig, GameOverModal } from "../modals";
import { Button, Box, Typography } from "@mui/material";
import { Settings } from "@mui/icons-material";

export const Game = () => {
  const { t, i18n } = useTranslation();
  const classes = useStyles();
  const [chosen, setChosen] = useState<string[]>([]);
  const [shuffledSentence, setShuffledSentence] = useState<string[]>([]);
  const [configModalOpen, setConfigModalOpen] = useState(false);
  const [gameOverModalOpen, setGameOverModalOpen] = useState(false);
  const [gameConfig, setGameConfig] = useState<GameConfig | null>(null);
  const [gameStarted, setGameStarted] = useState(false);
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

  const {
    speak,
    stop,
    isLoading: speechLoading,
    error: speechError,
  } = useAvatarSpeech({});

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
        console.log("No sentence or words available:", { sentence, words });
        return;
      }
      console.log("New sentence loaded:", sentence, "Words:", words);
      setChosen([]);
      setShuffledSentence(shuffleDistinct(words));
    };
    handleNewSentence();
  }, [sentence, words]);

  const handleConfigConfirm = (config: GameConfig) => {
    setGameConfig(config);
    setConfigModalOpen(false);
    // Reset game state when config changes
    setChosen([]);
    setShuffledSentence([]);
    setGameStarted(false);
    // Reset the hook's internal state
    resetGame();
  };

  const handleConfigChange = () => {
    setConfigModalOpen(true);
  };

  const handlePlay = () => {
    if (!sentence || sentence.trim() === "") {
      console.log("No sentence to play:", sentence);
      return;
    }
    if (speechLoading) {
      console.log("Speech is still loading, please wait");
      return;
    }
    console.log("Playing sentence:", sentence);
    speak(sentence);
  };

  const handleNextClick = async () => {
    stop();

    const result = await fetchSentence();

    // Check if we've completed all sentences
    if (!result || !result.sentence) {
      // Game is over - show game over modal
      setGameOverModalOpen(true);
      return;
    }
    setChosen([]);
    if (result.words && result.words.length > 0) {
      setShuffledSentence(shuffleDistinct(result.words));
    }
  };

  const handleGameOverPlayAgain = () => {
    setGameOverModalOpen(false);
    // Reset the hook's internal state
    resetGame();
    // Reset component state
    setChosen([]);
    setShuffledSentence([]);
    setGameStarted(false);
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
    setShuffledSentence((prev) => prev.filter((w) => w !== word));
    setChosen((prev) => [...prev, word]);
  };

  const handleUnchooseWord = (index: number, word: string) => {
    setChosen((prev) => prev.filter((_, i) => i !== index));
    setShuffledSentence((prev) => [word, ...prev]);
  };

  const isCorrect = useMemo(() => {
    if (words.length > 0) {
      return chosen.join(" ") === words.join(" ");
    }
  }, [chosen, words]);

  const handleCheck = () => {
    alert(isCorrect ? "Correct!" : "Try again!");
    return isCorrect;
  };

  const shuffleDistinct = (words: string[]) => {
    if (words.length < 2) return [...words];

    const original = words.join(" ");
    for (let i = 0; i < 5; i++) {
      const arr = [...words];
      for (let j = arr.length - 1; j > 0; j--) {
        const k = Math.floor(Math.random() * (j + 1));
        [arr[j], arr[k]] = [arr[k], arr[j]];
      }
      if (arr.join(" ") !== original) return arr;
    }
    return [...words].sort(() => Math.random() - 0.5);
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
      <>
        <Box className={classes.welcomeContainer}>
          <Typography
            className={classes.welcomeText}
            variant="body1"
            color="text.secondary"
          >
            {t("pages.wordOrderGame.welcome.description")}
          </Typography>
          <Button
            className={classes.welcomeButton}
            variant="contained"
            size="large"
            onClick={() => setConfigModalOpen(true)}
          >
            {t("pages.wordOrderGame.welcome.configure")}
          </Button>
        </Box>

        <GameConfigModal
          open={configModalOpen}
          onClose={() => setConfigModalOpen(false)}
          onConfirm={handleConfigConfirm}
          getDifficultyLevelLabel={getDifficultyLabel}
        />
      </>
    );
  }

  return (
    <>
      <div className={classes.gameContainer}>
        {/* Game Header with Settings */}
        <Box className={classes.gameHeader}>
          <Box className={classes.gameHeaderInfo}>
            <Typography variant="body2" color="text.secondary">
              {t("pages.wordOrderGame.current.difficulty")}:{" "}
              {getDifficultyLabel(gameConfig.difficulty)}
              {" | "}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {t("pages.wordOrderGame.current.nikud")}:{" "}
              {gameConfig.nikud
                ? t("pages.wordOrderGame.yes")
                : t("pages.wordOrderGame.no")}
              {" | "}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {t("pages.wordOrderGame.current.sentence")}:{" "}
              {currentSentenceIndex + 1}/{sentenceCount}
            </Typography>
          </Box>
          <Button
            variant="outlined"
            size="small"
            startIcon={<Settings />}
            className={classes.settingsButton + ' ' + (isHebrew ? classes.settingsButtonHebrew : '')}
            onClick={handleConfigChange}
          >
            {t("pages.wordOrderGame.settings")}
          </Button>
        </Box>

        <div className={classes.gameLogic}>
          <div className={classes.speakersContainer}>
            <Speaker
              onClick={() => handlePlay()}
              //disabled={!sentence || sentence.trim() === "" || speechLoading}
            />
          </div>

          <div className={classes.answerArea} dir="rtl">
            <div className={classes.dashLine} />
            <div className={classes.dashLineWithWords} data-testid="wog-chosen">
              {chosen.map((w, i) => (
                <button
                  key={`c-${w}-${i}`}
                  className={classes.chosenWord}
                  onClick={() => handleUnchooseWord(i, w)}
                >
                  {w}
                </button>
              ))}
            </div>
          </div>

          <div className={classes.wordsBank} dir="rtl" data-testid="wog-bank">
            {loading && <div>{t("pages.wordOrderGame.loading")}</div>}
            {error && <div style={{ color: "red" }}>{error}</div>}
            {!loading &&
              !error &&
              shuffledSentence.map((w, i) => (
                <button
                  key={`b-${w}-${i}`}
                  className={classes.bankWord}
                  onClick={() => handleChooseWord(w)}
                >
                  {w}
                </button>
              ))}
          </div>
        </div>

        <div className={classes.sideButtons}>
          <button data-testid="wog-reset" onClick={handleReset}>
            {t("pages.wordOrderGame.reset")}
          </button>
          <button data-testid="wog-check" onClick={handleCheck}>
            {t("pages.wordOrderGame.check")}
          </button>
          <button
            data-testid="wog-next"
            disabled={loading}
            onClick={handleNextClick}
          >
            {t("pages.wordOrderGame.next")}
          </button>
        </div>
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
        onClose={() => setGameOverModalOpen(false)}
        onPlayAgain={handleGameOverPlayAgain}
        onChangeSettings={handleGameOverChangeSettings}
        totalSentences={sentenceCount}
      />
    </>
  );
};
