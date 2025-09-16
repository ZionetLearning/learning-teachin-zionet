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

  // Determine if current language is Hebrew (RTL)
  const isHebrew = i18n.language === "he" || i18n.language === "heb";
  const direction = isHebrew ? "rtl" : "ltr";

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      sx={{
        "& .MuiDialog-paper": {
          borderRadius: 2,
          direction: direction,
        },
      }}
    >
      <DialogTitle>
        <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
          <Celebration color="primary" />
          <Typography variant="h5" component="div">
            {t("pages.wordOrderGame.gameOver.title")}
          </Typography>
        </Box>
      </DialogTitle>

      <DialogContent>
        <Box sx={{ textAlign: "center", py: 2 }}>
          <Typography variant="h6" gutterBottom>
            {t("pages.wordOrderGame.gameOver.congratulations")}
          </Typography>
          <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
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
        sx={{
          px: 3,
          pb: 3,
          gap: 1,
          direction: direction,
          justifyContent: "flex-start",
        }}
      >
        {isHebrew ? (
          // Hebrew: Use endIcon and push icons to the right edge
          <>
            <Button
              onClick={onPlayAgain}
              variant="contained"
              color="primary"
              startIcon={<Replay />}
              sx={{
                gap: 1.5,
                justifyContent: "space-between",
                "& .MuiButton-startIcon": { marginRight: -1.2 },
              }}
            >
              {t("pages.wordOrderGame.gameOver.playAgain")}
            </Button>
            <Button
              onClick={onChangeSettings}
              variant="outlined"
              startIcon={<Settings />}
              sx={{
                gap: 1.5,
                justifyContent: "space-between",
                "& .MuiButton-startIcon": { marginRight: -1.2 },
              }}
            >
              {t("pages.wordOrderGame.gameOver.changeSettings")}
            </Button>
            <Button onClick={onClose} color="inherit">
              {t("pages.wordOrderGame.close")}
            </Button>
          </>
        ) : (
          // English: Use startIcon and push icons to the left edge
          <>
            <Button
              onClick={onPlayAgain}
              variant="contained"
              color="primary"
              startIcon={<Replay />}
              sx={{

                gap: 1.5,
                justifyContent: "space-between",
                "& .MuiButton-startIcon": { marginRight: "auto" },
              }}
            >
              {t("pages.wordOrderGame.gameOver.playAgain")}
            </Button>
            <Button
              onClick={onChangeSettings}
              variant="outlined"
              startIcon={<Settings />}
              sx={{
                px: 1.5,
                gap: 1.5,
                justifyContent: "space-between",
                "& .MuiButton-startIcon": { marginRight: "auto" },
              }}
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
