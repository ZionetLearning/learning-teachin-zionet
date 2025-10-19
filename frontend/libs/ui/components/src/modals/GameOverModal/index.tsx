import { useCallback, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
  IconButton,
} from "@mui/material";
import { useTranslation } from "react-i18next";
import Celebration from "@mui/icons-material/Celebration";
import Replay from "@mui/icons-material/Replay";
import Settings from "@mui/icons-material/Settings";
import PendingActionsIcon from "@mui/icons-material/PendingActions";
import HomeIcon from "@mui/icons-material/Home";
import { useStyles } from "./style";

interface GameOverModalProps {
  open: boolean;
  onPlayAgain: () => void;
  onChangeSettings: () => void;
  correctSentences: number;
  totalSentences: number;
}

export const GameOverModal = ({
  open,
  onPlayAgain,
  onChangeSettings,
  correctSentences,
  totalSentences,
}: GameOverModalProps) => {
  const { t, i18n } = useTranslation();
  const classes = useStyles();
  const navigate = useNavigate();

  const isHebrew = i18n.language === "he" || i18n.language === "heb";
  const dirDialogClass = isHebrew
    ? classes.gameOverModalRtl
    : classes.gameOverModalLtr;
  const actionsClass = isHebrew
    ? classes.gameOverActionsRtl
    : classes.gameOverActionsLtr;
  const buttonClass = isHebrew
    ? classes.gameOverButtonHebrew
    : classes.gameOverButtonEnglish;

  const onViewMistakes = useCallback(
    () => navigate("/practice-mistakes"),
    [navigate],
  );

  const onGoHome = useCallback(() => navigate("/"), [navigate]);

  const isPerfect = correctSentences === totalSentences;

  const actionButtons = useMemo(
    () => [
      {
        key: "play-again",
        testId: "typing-play-again",
        onClick: onPlayAgain,
        variant: "contained" as const,
        color: "primary" as const,
        icon: <Replay />,
        label: t("pages.wordOrderGame.gameOver.playAgain"),
      },
      {
        key: "change-settings",
        testId: "typing-change-settings",
        onClick: onChangeSettings,
        variant: "outlined" as const,
        icon: <Settings />,
        label: t("pages.wordOrderGame.gameOver.changeSettings"),
      },
      {
        key: "view-mistakes",
        testId: "typing-view-mistakes",
        onClick: onViewMistakes,
        variant: "outlined" as const,
        icon: <PendingActionsIcon />,
        label: t("pages.wordOrderGame.gameOver.viewMyMistakes"),
      },
    ],
    [onPlayAgain, onChangeSettings, onViewMistakes, t],
  );

  return (
    <Dialog open={open} maxWidth="md" className={dirDialogClass}>
      <DialogTitle>
        <Box className={classes.gameOverTitle}>
          <Celebration color="primary" />
          <Typography variant="h5" component="div">
            {t("pages.wordOrderGame.gameOver.title")}
          </Typography>
        </Box>
      </DialogTitle>

      <DialogContent>
        <Box className={classes.gameOverContent}>
          {isPerfect ? (
            <>
              <Typography variant="h6" gutterBottom>
                {t("pages.wordOrderGame.gameOver.congratulations")}
              </Typography>
              <Typography
                variant="body1"
                color="text.secondary"
                className={classes.gameOverCompletedText}
              >
                {t("pages.wordOrderGame.gameOver.completed", {
                  correct: correctSentences,
                  total: totalSentences,
                })}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {t("pages.wordOrderGame.gameOver.whatNext")}
              </Typography>
            </>
          ) : (
            <>
              <Typography variant="h6" gutterBottom>
                {t("pages.wordOrderGame.gameOver.completed", {
                  correct: correctSentences,
                  total: totalSentences,
                })}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {t("pages.wordOrderGame.gameOver.whatNext")}
              </Typography>
            </>
          )}
        </Box>
      </DialogContent>

      <DialogActions className={actionsClass}>
        {actionButtons.map((b) => (
          <Button
            key={b.key}
            data-testid={b.testId}
            onClick={b.onClick}
            variant={b.variant}
            {...(b.color ? { color: b.color } : {})}
            startIcon={b.icon}
            className={buttonClass}
          >
            {b.label}
          </Button>
        ))}
        <IconButton
          data-testid="typing-go-home"
          onClick={onGoHome}
          color="primary"
        >
          <HomeIcon />
        </IconButton>
      </DialogActions>
    </Dialog>
  );
};
