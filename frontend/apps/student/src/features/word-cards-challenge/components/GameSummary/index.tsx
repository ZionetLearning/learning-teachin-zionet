import { Box, Button, Typography } from "@mui/material";
import { useTranslation } from "react-i18next";
import { useMemo } from "react";
import { useStyles } from "./style";

type GameMode = "heb-to-eng" | "eng-to-heb";

interface GameSummaryProps {
  correctCount: number;
  totalCards: number;
  currentMode: GameMode;
  onPlayAgain: (mode: GameMode) => void;
}

export const GameSummary = ({
  correctCount,
  totalCards,
  currentMode,
  onPlayAgain,
}: GameSummaryProps) => {
  const classes = useStyles();
  const { t } = useTranslation();

  const percentage = useMemo(() => {
    if (totalCards === 0) return 0;
    return Math.round((correctCount / totalCards) * 100);
  }, [correctCount, totalCards]);

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
          <Typography className={classes.scoreNumber}>{percentage}%</Typography>
          <Typography className={classes.scoreDetails}>
            {t("pages.wordCardsChallenge.correctAnswers", {
              correct: correctCount,
              total: totalCards,
            })}
          </Typography>
        </Box>
        <Box className={classes.summaryButtons}>
          <Button
            variant="contained"
            className={classes.summaryButton}
            onClick={() => onPlayAgain(currentMode)}
          >
            {t("pages.wordCardsChallenge.playAgain")}
          </Button>
          <Button
            variant="outlined"
            className={classes.summaryButtonOutlined}
            onClick={() =>
              onPlayAgain(
                currentMode === "heb-to-eng" ? "eng-to-heb" : "heb-to-eng",
              )
            }
          >
            {t("pages.wordCardsChallenge.switchDirection")}
          </Button>
        </Box>
      </Box>
    </Box>
  );
};
