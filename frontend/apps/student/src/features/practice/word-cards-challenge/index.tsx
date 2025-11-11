import { useCallback, useEffect, useRef, useState } from "react";
import {
  Box,
  Button,
  CircularProgress,
  Dialog,
  TextField,
  Typography,
} from "@mui/material";
import { useTranslation } from "react-i18next";
import { useGetWordCards, type WordCard } from "@student/api";
import {
  ContextAwareChat,
  useWordCardsContext,
} from "@ui-components/ContextAwareChat";
import { useStyles } from "./style";
import { ModeSelection, GameSummary } from "./components";
import { FEEDBACK_DISPLAY_DURATION } from "./constants";

type GameMode = "heb-to-eng" | "eng-to-heb";
type GameState = "mode-selection" | "playing" | "summary";

export const WordCardsChallenge = () => {
  const classes = useStyles();
  const { t } = useTranslation();
  const timeoutRef = useRef<NodeJS.Timeout | null>(null);

  const [gameState, setGameState] = useState<GameState>("mode-selection");
  const [mode, setMode] = useState<GameMode>("heb-to-eng");
  const [currentIndex, setCurrentIndex] = useState(0);
  const [userAnswer, setUserAnswer] = useState("");
  const [feedback, setFeedback] = useState<"correct" | "wrong" | null>(null);
  const [displayedCorrectAnswer, setDisplayedCorrectAnswer] = useState("");
  const [correctCount, setCorrectCount] = useState(0);
  const [shuffledCards, setShuffledCards] = useState<WordCard[]>([]);

  const { data: cards, isLoading, isError } = useGetWordCards();

  useEffect(
    function resetOnCardsChange() {
      if (gameState === "playing" && shuffledCards.length === 0 && cards) {
        setGameState("mode-selection");
      }
    },
    [cards, gameState, shuffledCards.length],
  );

  useEffect(function cleanupTimeout() {
    return () => {
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current);
      }
    };
  }, []);

  const shuffleCards = useCallback((cardList: WordCard[]) => {
    const shuffled = [...cardList];
    for (let i = shuffled.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [shuffled[i], shuffled[j]] = [shuffled[j], shuffled[i]];
    }
    return shuffled;
  }, []);

  const startGame = useCallback(
    (selectedMode: GameMode) => {
      if (!cards || cards.length === 0) return;
      setMode(selectedMode);
      setShuffledCards(shuffleCards(cards));
      setCurrentIndex(0);
      setCorrectCount(0);
      setUserAnswer("");
      setFeedback(null);
      setGameState("playing");
    },
    [cards, shuffleCards],
  );

  const hasCards = (cards?.length ?? 0) > 0;
  const currentCard = shuffledCards[currentIndex];

  const checkAnswer = useCallback(() => {
    const correctAnswer = currentCard
      ? mode === "heb-to-eng"
        ? currentCard.english
        : currentCard.hebrew
      : "";
    const trimmedAnswer = userAnswer.trim().toLowerCase();
    const trimmedCorrect = correctAnswer.trim().toLowerCase();
    const isCorrect = trimmedAnswer === trimmedCorrect;

    setDisplayedCorrectAnswer(correctAnswer);
    setFeedback(isCorrect ? "correct" : "wrong");
    if (isCorrect) {
      setCorrectCount((prev) => prev + 1);
    }

    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current);
    }

    timeoutRef.current = setTimeout(() => {
      if (currentIndex + 1 >= shuffledCards.length) {
        setGameState("summary");
        setFeedback(null);
      } else {
        setCurrentIndex((prev) => prev + 1);
        setUserAnswer("");
        setFeedback(null);
      }
    }, FEEDBACK_DISPLAY_DURATION);
  }, [userAnswer, currentCard, mode, currentIndex, shuffledCards.length]);

  const pageContext = useWordCardsContext({
    currentExercise: currentIndex + 1,
    totalExercises: shuffledCards.length,
    question:
      currentCard && mode === "heb-to-eng"
        ? currentCard.hebrew
        : currentCard?.english,
    correctAnswer:
      currentCard && mode === "heb-to-eng"
        ? currentCard.english
        : currentCard?.hebrew,
    userAttempt: userAnswer,
    currentWord: currentCard
      ? {
          hebrew: currentCard.hebrew,
          english: currentCard.english,
        }
      : undefined,
    additionalContext: {
      mode,
      gameState,
      correctCount,
      feedback,
    },
  });

  if (isLoading) {
    return (
      <Box className={classes.centerState}>
        <CircularProgress />
        <Typography>{t("pages.wordCardsChallenge.loading")}</Typography>
      </Box>
    );
  }

  if (isError) {
    return (
      <Box className={classes.emptyState}>
        <Typography className={classes.emptyTitle}>
          {t("pages.wordCardsChallenge.errorTitle")}
        </Typography>
        <Typography className={classes.emptyDescription}>
          {t("pages.wordCardsChallenge.errorDescription")}
        </Typography>
      </Box>
    );
  }

  if (!hasCards) {
    return (
      <Box className={classes.emptyState}>
        <Typography className={classes.emptyTitle}>
          {t("pages.wordCardsChallenge.noCardsTitle")}
        </Typography>
        <Typography className={classes.emptyDescription}>
          {t("pages.wordCardsChallenge.noCardsDescription")}
        </Typography>
      </Box>
    );
  }

  if (gameState === "mode-selection") {
    return <ModeSelection onStartGame={startGame} />;
  }

  if (gameState === "summary") {
    return (
      <GameSummary
        correctCount={correctCount}
        totalCards={shuffledCards.length}
        currentMode={mode}
        onPlayAgain={startGame}
      />
    );
  }

  const questionText = currentCard
    ? mode === "heb-to-eng"
      ? currentCard.hebrew
      : currentCard.english
    : "";

  return (
    <Box className={classes.container}>
      <Box
        className={`${classes.gameCard} ${
          feedback === "correct"
            ? classes.gameCardSlideRight
            : feedback === "wrong"
              ? classes.gameCardSlideLeft
              : ""
        }`}
      >
        <Box className={classes.progressBar}>
          <Typography className={classes.progressText}>
            {t("pages.wordCardsChallenge.cardProgress", {
              current: currentIndex + 1,
              total: shuffledCards.length,
            })}
          </Typography>
        </Box>

        <Box className={classes.questionBox}>
          <Typography className={classes.questionLabel}>
            {mode === "heb-to-eng"
              ? t("pages.wordCardsChallenge.translateToEnglish")
              : t("pages.wordCardsChallenge.translateToHebrew")}
          </Typography>
          <Typography
            className={classes.questionWord}
            dir={mode === "heb-to-eng" ? "rtl" : "ltr"}
          >
            {questionText}
          </Typography>
        </Box>

        <Box className={classes.answerBox}>
          <TextField
            fullWidth
            autoFocus
            value={userAnswer}
            onChange={(e) => setUserAnswer(e.target.value)}
            placeholder={t("pages.wordCardsChallenge.typeYourAnswer")}
            disabled={feedback !== null}
            className={classes.textField}
            onKeyDown={(e) => {
              if (e.key === "Enter" && userAnswer.trim() && !feedback) {
                e.preventDefault();
                checkAnswer();
              }
            }}
            slotProps={{
              htmlInput: {
                dir: mode === "eng-to-heb" ? "rtl" : "ltr",
              },
            }}
          />

          {feedback === null && (
            <Button
              variant="contained"
              className={classes.submitButton}
              onClick={checkAnswer}
              disabled={!userAnswer.trim()}
            >
              {t("pages.wordCardsChallenge.submit")}
            </Button>
          )}
        </Box>
      </Box>

      <Dialog
        open={feedback !== null}
        className={classes.feedbackModal}
        slotProps={{
          paper: {
            className:
              feedback === "correct"
                ? classes.feedbackModalCorrect
                : feedback === "wrong"
                  ? classes.feedbackModalWrong
                  : "",
          },
        }}
      >
        <Box className={classes.feedbackModalContent}>
          {feedback === "correct" ? (
            <Typography className={classes.feedbackModalText}>
              {t("pages.wordCardsChallenge.correct")}
            </Typography>
          ) : feedback === "wrong" ? (
            <Box>
              <Typography className={classes.feedbackModalText}>
                {t("pages.wordCardsChallenge.wrongLabel")}
              </Typography>
              <Typography
                className={classes.feedbackModalText}
                dir={mode === "eng-to-heb" ? "rtl" : "ltr"}
              >
                {displayedCorrectAnswer}
              </Typography>
            </Box>
          ) : null}
        </Box>
      </Dialog>

      <ContextAwareChat pageContext={pageContext} />
    </Box>
  );
};
