import { useEffect, useMemo, useState } from "react";
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  TextField,
  Typography,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import { useTranslation } from "react-i18next";
import { useCreateWordCard, type CreateWordCardRequest } from "../../api";
import { useWordExplanation, useTrackAchievement } from "../../hooks";
import { ACHIEVEMENT_INCREMENT } from "../../constants";
import { useStyles } from "./style";

export type AddWordCardDialogProps = {
  open: boolean;
  onClose: () => void;
  initialHebrew?: string;
  context?: string;
};

export const AddWordCardDialog = ({
  open,
  onClose,
  initialHebrew,
  context,
}: AddWordCardDialogProps) => {
  const classes = useStyles();
  const { t } = useTranslation();

  const createCard = useCreateWordCard();
  const { track } = useTrackAchievement("WordCards");

  const selectionMode = useMemo(
    () => Boolean(initialHebrew?.trim()),
    [initialHebrew],
  );

  const [hebrew, setHebrew] = useState<string>(initialHebrew?.trim() ?? "");
  const [english, setEnglish] = useState<string>("");

  const {
    explanation,
    isLoading: isLoadingExplanation,
    reset: resetExplanation,
  } = useWordExplanation(hebrew, context ?? "", open && selectionMode);

  // keep state in sync when dialog opens or word changes
  useEffect(() => {
    if (open) {
      setHebrew(initialHebrew?.trim() ?? "");
      setEnglish("");
      resetExplanation();
    }
  }, [initialHebrew, open, resetExplanation]);

  const disabled = useMemo(() => {
    if (selectionMode) {
      return english.trim().length === 0 || createCard.isPending;
    }
    return (
      hebrew.trim().length === 0 ||
      english.trim().length === 0 ||
      createCard.isPending
    );
  }, [selectionMode, hebrew, english, createCard.isPending]);

  const title = selectionMode
    ? t("pages.wordCards.addTranslation")
    : t("pages.wordCards.addCard");

  const handleSave = () => {
    const body: CreateWordCardRequest = {
      hebrew: hebrew.trim(),
      english: english.trim(),
      context: context?.trim(),
      explanation: explanation?.trim(),
    };
    createCard.mutate(body, {
      onSuccess: () => {
        track(ACHIEVEMENT_INCREMENT);
        setEnglish("");
        resetExplanation();
        if (!selectionMode) setHebrew("");
        onClose();
      },
    });
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="xs"
      fullWidth
      slotProps={{ paper: { className: classes.dialogPaper } }}
    >
      <DialogTitle className={classes.dialogTitle}>
        {title}
        <IconButton
          onClick={onClose}
          aria-label="close"
          className={classes.closeButton}
        >
          <CloseIcon />
        </IconButton>
      </DialogTitle>

      <DialogContent>
        <Box className={classes.dialogBodyGradient}>
          {selectionMode ? (
            <>
              <Box className={classes.wordPanel}>
                <Typography className={classes.wordLabel}>
                  {t("pages.wordCards.hebrewWord")}
                </Typography>
                <Typography className={classes.hebrewWord}>{hebrew}</Typography>
              </Box>

              <TextField
                label={t("pages.wordCards.englishTranslation")}
                fullWidth
                autoFocus
                className={classes.textField}
                value={english}
                onChange={(e) => setEnglish(e.target.value)}
                placeholder={t("pages.wordCards.typeTheEnglishTranslation")}
                onKeyDown={(e) => {
                  if (e.key === "Enter" && !disabled) {
                    e.preventDefault();
                    handleSave();
                  }
                }}
              />
              {isLoadingExplanation && (
                <Typography className={classes.loadingText}>
                  {t("pages.wordCards.generatingExplanation")}
                </Typography>
              )}
              {explanation && (
                <Box className={classes.explanationBox}>
                  <Typography className={classes.explanationText}>
                    {explanation}
                  </Typography>
                </Box>
              )}
            </>
          ) : (
            <Box className={classes.wordPanel}>
              <TextField
                label={t("pages.wordCards.hebrew")}
                fullWidth
                autoFocus
                value={hebrew}
                className={classes.textField}
                onChange={(e) => setHebrew(e.target.value)}
                placeholder={t("pages.wordCards.enterTheHebrewWord")}
                slotProps={{
                  htmlInput: {
                    dir: "rtl",
                  },
                }}
                sx={{ mb: 1.5 }}
              />
              <TextField
                label={t("pages.wordCards.english")}
                fullWidth
                value={english}
                className={classes.textField}
                onChange={(e) => setEnglish(e.target.value)}
                placeholder={t("pages.wordCards.enterTheEnglishTranslation")}
                onKeyDown={(e) => {
                  if (e.key === "Enter" && !disabled) {
                    e.preventDefault();
                    handleSave();
                  }
                }}
              />
            </Box>
          )}
        </Box>
      </DialogContent>

      <DialogActions className={classes.actions}>
        <Button
          onClick={onClose}
          variant="text"
          className={classes.cancelButton}
        >
          {t("pages.wordCards.cancel")}
        </Button>
        <Button
          onClick={handleSave}
          variant="contained"
          disabled={disabled}
          className={classes.saveButton}
        >
          {createCard.isPending
            ? t("pages.wordCards.saving")
            : t("pages.wordCards.save")}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
