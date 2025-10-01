import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
} from "@mui/material";
import { useTranslation } from "react-i18next";
import Celebration from "@mui/icons-material/Celebration";
import Replay from "@mui/icons-material/Replay";
import Settings from "@mui/icons-material/Settings";
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
  const isHebrew = i18n.language === "he" || i18n.language === "heb";

  return (
    <Dialog
      open={open}
      maxWidth="sm"
      fullWidth
      className={isHebrew ? classes.gameOverModalRtl : classes.gameOverModalLtr}
    >
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
          {correctSentences === totalSentences ? (
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

      <DialogActions
        className={
          isHebrew ? classes.gameOverActionsRtl : classes.gameOverActionsLtr
        }
      >
        {isHebrew ? (
          <>
            <Button
              data-testid="typing-play-again"
              onClick={onPlayAgain}
              variant="contained"
              color="primary"
              startIcon={<Replay />}
              className={classes.gameOverButtonHebrew}
            >
              {t("pages.wordOrderGame.gameOver.playAgain")}
            </Button>
            <Button
              data-testid="typing-change-settings"
              onClick={onChangeSettings}
              variant="outlined"
              startIcon={<Settings />}
              className={classes.gameOverButtonHebrew}
            >
              {t("pages.wordOrderGame.gameOver.changeSettings")}
            </Button>
          </>
        ) : (
          <>
            <Button
              data-testid="typing-play-again"
              onClick={onPlayAgain}
              variant="contained"
              color="primary"
              startIcon={<Replay />}
              className={classes.gameOverButtonEnglish}
            >
              {t("pages.wordOrderGame.gameOver.playAgain")}
            </Button>
            <Button
              data-testid="typing-change-settings"
              onClick={onChangeSettings}
              variant="outlined"
              startIcon={<Settings />}
              className={classes.gameOverButtonEnglish}
            >
              {t("pages.wordOrderGame.gameOver.changeSettings")}
            </Button>
          </>
        )}
      </DialogActions>
    </Dialog>
  );
};
