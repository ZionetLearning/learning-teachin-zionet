import { useCallback } from "react";
import { useNavigate } from "react-router-dom";
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
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import ErrorIcon from "@mui/icons-material/Error";
import RefreshIcon from "@mui/icons-material/Refresh";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { useStyles } from "./style";

interface RetryResultModalProps {
  open: boolean;
  isCorrect: boolean;
  onRetryAgain: () => void;
  onBackToMistakes: () => void;
}

export const RetryResultModal = ({
  open,
  isCorrect,
  onRetryAgain,
  onBackToMistakes,
}: RetryResultModalProps) => {
  const { t, i18n } = useTranslation();
  const classes = useStyles();
  const navigate = useNavigate();

  const isHebrew = i18n.language === "he" || i18n.language === "heb";

  const handleBackToMistakes = useCallback(() => {
    onBackToMistakes();
    navigate("/practice-mistakes");
  }, [onBackToMistakes, navigate]);

  const handleRetryAgain = useCallback(() => {
    onRetryAgain();
  }, [onRetryAgain]);

  return (
    <Dialog
      open={open}
      maxWidth="sm"
      fullWidth
      className={isHebrew ? classes.modalRtl : classes.modalLtr}
      dir={isHebrew ? "rtl" : "ltr"}
    >
      <DialogTitle className={classes.title}>
        <Box className={classes.titleIconBox}>
          {isCorrect ? (
            <CheckCircleIcon color="success" className={classes.titleIcon} />
          ) : (
            <ErrorIcon color="error" className={classes.titleIcon} />
          )}
        </Box>
        <Typography variant="h5" component="div" className={classes.titleText}>
          {isCorrect
            ? t("pages.retryResultModal.successTitle")
            : t("pages.retryResultModal.failureTitle")}
        </Typography>
      </DialogTitle>

      <DialogContent className={classes.content}>
        <Box className={classes.contentBox}>
          <Typography variant="h6" gutterBottom>
            {isCorrect
              ? t("pages.retryResultModal.successMessage")
              : t("pages.retryResultModal.failureMessage")}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {isCorrect
              ? t("pages.retryResultModal.successDescription")
              : t("pages.retryResultModal.failureDescription")}
          </Typography>
        </Box>
      </DialogContent>

      <DialogActions className={classes.actions}>
        {!isCorrect && (
          <Button
            onClick={handleRetryAgain}
            variant="outlined"
            startIcon={<RefreshIcon />}
            className={classes.button}
          >
            {t("pages.retryResultModal.tryAgain")}
          </Button>
        )}
        <Button
          onClick={handleBackToMistakes}
          variant="contained"
          startIcon={<ArrowBackIcon />}
          className={classes.button}
        >
          {t("pages.retryResultModal.backToMistakes")}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
