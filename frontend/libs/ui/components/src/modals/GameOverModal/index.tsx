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
import { Celebration, Settings, Replay } from "@mui/icons-material";
import { useStyles } from "./style";
interface GameOverModalProps {
  open: boolean;
  onClose: () => void;
  onPlayAgain: () => void;
  onChangeSettings: () => void;
  totalSentences: number;
}

export const GameOverModal = ({
  open,
  onClose,
  onPlayAgain,
  onChangeSettings,
  totalSentences,
}: GameOverModalProps) => {
  const { t, i18n } = useTranslation();
  const classes = useStyles();
  // Determine if current language is Hebrew (RTL)
  const isHebrew = i18n.language === "he" || i18n.language === "heb";

  return (
    <Dialog
      open={open}
      onClose={onClose}
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
          <Typography variant="h6" gutterBottom>
            {t("pages.wordOrderGame.gameOver.congratulations")}
          </Typography>
          <Typography
            variant="body1"
            color="text.secondary"
            className={classes.gameOverCompletedText}
          >
            {t("pages.wordOrderGame.gameOver.completed", {
              count: totalSentences,
            })}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {t("pages.wordOrderGame.gameOver.whatNext")}
          </Typography>
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
              onClick={onPlayAgain}
              variant="contained"
              color="primary"
              startIcon={<Replay />}
              className={classes.gameOverButtonHebrew}
            >
              {t("pages.wordOrderGame.gameOver.playAgain")}
            </Button>
            <Button
              onClick={onChangeSettings}
              variant="outlined"
              startIcon={<Settings />}
              className={classes.gameOverButtonHebrew}
            >
              {t("pages.wordOrderGame.gameOver.changeSettings")}
            </Button>
            <Button onClick={onClose} color="inherit">
              {t("pages.wordOrderGame.close")}
            </Button>
          </>
        ) : (
          <>
            <Button
              onClick={onPlayAgain}
              variant="contained"
              color="primary"
              startIcon={<Replay />}
              className={classes.gameOverButtonEnglish}
            >
              {t("pages.wordOrderGame.gameOver.playAgain")}
            </Button>
            <Button
              onClick={onChangeSettings}
              variant="outlined"
              startIcon={<Settings />}
              className={classes.gameOverButtonEnglish}
            >
              {t("pages.wordOrderGame.gameOver.changeSettings")}
            </Button>
            <Button onClick={onClose} color="inherit">
              {t("pages.wordOrderGame.close")}
            </Button>
          </>
        )}
      </DialogActions>
    </Dialog>
  );
};