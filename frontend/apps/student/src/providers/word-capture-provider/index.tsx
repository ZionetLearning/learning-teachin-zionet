import React, { useEffect, useState } from "react";
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
import { useCreateWordCard, type CreateWordCardRequest } from "../../api";
import { useStyles } from "./style";

type WordCaptureProviderProps = {
  children: React.ReactNode;
  buildRequest: (
    hebrewWord: string,
    englishTranslation: string,
  ) => CreateWordCardRequest;
  minLength?: number;
  maxLength?: number;
};

export const WordCaptureProvider: React.FC<WordCaptureProviderProps> = ({
  children,
  buildRequest,
  minLength = 1,
  maxLength = 20,
}) => {
  const classes = useStyles();

  const [open, setOpen] = useState(false);
  const [hebrewWord, setHebrewWord] = useState<string>("");
  const [english, setEnglish] = useState<string>("");

  const createWordCard = useCreateWordCard();

  // Detect a single Hebrew "word" in a selection.
  const extractHebrewWord = (raw: string): string | null => {
    const trimmed = raw.trim();
    if (trimmed.length < minLength || trimmed.length > maxLength) return null;

    // Match a single contiguous Hebrew range (including geresh/maqaf where common)
    const match = trimmed.match(/[\u0590-\u05FF\u05BE\u05F3\u05F4]+/);
    if (!match) return null;

    const word = match[0];
    // Filter out multi-word selections (spaces) and numbers/punctuation-only cases
    if (/\s/.test(trimmed)) return null;
    // basic sanity: ensure the match is "most" of the selection
    if (word.length < Math.min(trimmed.length, maxLength) * 0.5) return null;

    return word;
  };

  // mouseup listener to catch selections
  useEffect(() => {
    const handleMouseUp = () => {
      const selection = window.getSelection();
      const text = selection ? selection.toString() : "";
      if (!text) return;

      const word = extractHebrewWord(text);
      if (word) {
        setHebrewWord(word);
        setEnglish("");
        setOpen(true);
        // Clear selection
        selection?.removeAllRanges();
      }
    };

    document.addEventListener("mouseup", handleMouseUp);
    return () => document.removeEventListener("mouseup", handleMouseUp);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleClose = () => {
    setOpen(false);
    setHebrewWord("");
    setEnglish("");
  };

  const handleSave = () => {
    const request = buildRequest(hebrewWord, english.trim());
    createWordCard.mutate(request, {
      onSuccess: () => {
        handleClose();
      },
    });
  };

  const saveDisabled =
    english.trim().length === 0 ||
    createWordCard.isPending ||
    hebrewWord.length === 0;

  return (
    <Box sx={{ display: "contents" }}>
      {children}

      <Dialog
        open={open}
        onClose={handleClose}
        maxWidth="xs"
        fullWidth
        slotProps={{ paper: { className: classes.dialogPaper } }}
      >
        <DialogTitle className={classes.dialogTitle}>
          Add Translation
          <IconButton
            onClick={handleClose}
            aria-label="close"
            className={classes.closeButton}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>

        <DialogContent>
          <Box className={classes.dialogBodyGradient}>
            <Box className={classes.wordPanel}>
              <Box className={classes.wordMeta}>
                <Typography className={classes.wordLabel}>
                  Hebrew word
                </Typography>
                <Typography className={classes.hebrewWord}>
                  {hebrewWord}
                </Typography>
              </Box>
            </Box>

            <TextField
              label="English translation"
              fullWidth
              autoFocus
              className={classes.textField}
              value={english}
              onChange={(e) => setEnglish(e.target.value)}
              placeholder="Type the English meaning…"
              onKeyDown={(e) => {
                if (e.key === "Enter" && !saveDisabled) {
                  e.preventDefault();
                  handleSave();
                }
              }}
            />
          </Box>
        </DialogContent>

        <DialogActions className={classes.actions}>
          <Button
            onClick={handleClose}
            variant="text"
            className={classes.cancelButton}
          >
            Cancel
          </Button>
          <Button
            onClick={handleSave}
            variant="contained"
            disabled={saveDisabled}
            className={classes.saveButton}
          >
            {createWordCard.isPending ? "Saving…" : "Save"}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};
