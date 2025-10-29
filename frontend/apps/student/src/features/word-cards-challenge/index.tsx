import { useCallback, useEffect, useMemo, useState } from "react";
import {
  Box,
  Button,
  CircularProgress,
  TextField,
  Typography,
} from "@mui/material";
import { useTranslation } from "react-i18next";
import { useGetWordCards, type WordCard } from "@student/api";
import { useStyles } from "./style";

type GameMode = "heb-to-eng" | "eng-to-heb";
type GameState = "mode-selection" | "playing" | "summary";

export const WordCardsChallenge = () => {
  const classes = useStyles();
  const { t } = useTranslation();

  const [gameState, setGameState] = useState<GameState>("mode-selection");
  const [mode, setMode] = useState<GameMode>("heb-to-eng");
  const [currentIndex, setCurrentIndex] = useState(0);
  const [userAnswer, setUserAnswer] = useState("");
  const [feedback, setFeedback] = useState<"correct" | "wrong" | null>(null);
  const [correctCount, setCorrectCount] = useState(0);
  const [shuffledCards, setShuffledCards] = useState<WordCard[]>([]);

  const { data: cards, isLoading } = useGetWordCards();

  useEffect(
    function resetOnCardsChange() {
      if (gameState === "playing" && shuffledCards.length === 0 && cards) {
        setGameState("mode-selection");
      }
    },
    [cards, gameState, shuffledCards.length],
  );

  const shuffleCards = useCallback((cardList: WordCard[]) => {
    return [...cardList].sort(() => Math.random() - 0.5);
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

  const questionText = currentCard
    ? mode === "heb-to-eng"
      ? currentCard.hebrew
      : currentCard.english
    : "";

  const correctAnswer = currentCard
    ? mode === "heb-to-eng"
      ? currentCard.english
      : currentCard.hebrew
    : "";

  const checkAnswer = useCallback(() => {
    const trimmedAnswer = userAnswer.trim().toLowerCase();
    const trimmedCorrect = correctAnswer.trim().toLowerCase();
    const isCorrect = trimmedAnswer === trimmedCorrect;

    setFeedback(isCorrect ? "correct" : "wrong");
    if (isCorrect) {
      setCorrectCount((prev) => prev + 1);
    }

    setTimeout(() => {
      if (currentIndex + 1 >= shuffledCards.length) {
        setGameState("summary");
      } else {
        setCurrentIndex((prev) => prev + 1);
        setUserAnswer("");
        setFeedback(null);
      }
    }, 1500);
  }, [userAnswer, correctAnswer, currentIndex, shuffledCards.length]);

  const percentage = useMemo(() => {
    if (shuffledCards.length === 0) return 0;
    return Math.round((correctCount / shuffledCards.length) * 100);
  }, [correctCount, shuffledCards.length]);

  if (isLoading) {
    return (
      <Box className={classes.centerState}>
        <CircularProgress />
        <Typography>{t("pages.wordCardsChallenge.loading")}</Typography>
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
    return (
      <Box className={classes.container}>
        <Box className={classes.modeSelection}>
          <Typography className={classes.title}>
            {t("pages.wordCardsChallenge.title")}
          </Typography>
          <Typography className={classes.subtitle}>
            {t("pages.wordCardsChallenge.selectMode")}
          </Typography>
          <Box className={classes.modeButtons}>
            <Button
              variant="contained"
              className={classes.modeButton}
              onClick={() => startGame("heb-to-eng")}
            >
              {t("pages.wordCardsChallenge.hebToEng")}
            </Button>
            <Button
              variant="contained"
              className={classes.modeButton}
              onClick={() => startGame("eng-to-heb")}
            >
              {t("pages.wordCardsChallenge.engToHeb")}
            </Button>
          </Box>
        </Box>
      </Box>
    );
  }

  if (gameState === "summary") {
    return (
      <Box className={classes.container}>
        <Box className={classes.summary}>
          <Typography className={classes.summaryTitle}>
            {t("pages.wordCardsChallenge.gameComplete")}
          </Typography>
          <Box className={classes.scoreBox}>
            <Typography className={classes.scoreText}>
              {t("pages.wordCardsChallenge.yourScore")}
            </Typography>
            <Typography className={classes.scoreNumber}>
              {percentage}%
            </Typography>
            <Typography className={classes.scoreDetails}>
              {t("pages.wordCardsChallenge.correctAnswers", {
                correct: correctCount,
                total: shuffledCards.length,
              })}
            </Typography>
          </Box>
          <Box className={classes.summaryButtons}>
            <Button
              variant="contained"
              className={classes.summaryButton}
              onClick={() => startGame(mode)}
            >
              {t("pages.wordCardsChallenge.playAgain")}
            </Button>
            <Button
              variant="outlined"
              className={classes.summaryButtonOutlined}
              onClick={() =>
                startGame(mode === "heb-to-eng" ? "eng-to-heb" : "heb-to-eng")
              }
            >
              {t("pages.wordCardsChallenge.switchDirection")}
            </Button>
          </Box>
        </Box>
      </Box>
    );
  }

  return (
    <Box className={classes.container}>
      <Box className={classes.gameCard}>
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

        {feedback && (
          <Box
            className={
              feedback === "correct"
                ? classes.feedbackCorrect
                : classes.feedbackWrong
            }
          >
            <Typography
              className={classes.feedbackText}
              dir={mode === "heb-to-eng" ? "ltr" : "rtl"}
            >
              {feedback === "correct"
                ? t("pages.wordCardsChallenge.correct")
                : t("pages.wordCardsChallenge.wrong", {
                    answer: correctAnswer,
                  })}
            </Typography>
          </Box>
        )}
      </Box>
    </Box>
  );
};
