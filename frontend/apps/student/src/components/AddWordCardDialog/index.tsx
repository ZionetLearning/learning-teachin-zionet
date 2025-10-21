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

import { useCreateWordCard, type CreateWordCardRequest } from "../../api";
import { useStyles } from "./style";

export type AddWordCardDialogProps = {
  open: boolean;
  onClose: () => void;

  /**
   * If provided, dialog works in "selection mode":
   * - Title: "Add Translation"
   * - Shows the Hebrew word panel
   * - Single input for English translation
   * If omitted, dialog works in "blank mode":
   * - Title: "Add Card"
   * - Shows Hebrew + English inputs
   */
  initialHebrew?: string;

  /** Optional: called after successful creation */
  onCreated?: () => void;
};

export const AddWordCardDialog = ({
  open,
  onClose,
  initialHebrew,
  onCreated,
}: AddWordCardDialogProps) => {
  const classes = useStyles();
  const createCard = useCreateWordCard();

  const selectionMode = useMemo(
    () => Boolean(initialHebrew?.trim()),
    [initialHebrew],
  );

  const [hebrew, setHebrew] = useState<string>(initialHebrew?.trim() ?? "");
  const [english, setEnglish] = useState<string>("");

  // keep state in sync if initialHebrew changes between opens
  useEffect(() => {
    setHebrew(initialHebrew?.trim() ?? "");
    setEnglish("");
  }, [initialHebrew, open]);

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

  const title = selectionMode ? "Add Translation" : "Add Card";

  const handleSave = () => {
    const body: CreateWordCardRequest = {
      hebrew: hebrew.trim(),
      english: english.trim(),
    };
    createCard.mutate(body, {
      onSuccess: () => {
        setEnglish("");
        if (!selectionMode) setHebrew("");
        onCreated?.();
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
                  Hebrew word
                </Typography>
                <Typography className={classes.hebrewWord}>{hebrew}</Typography>
              </Box>

              <TextField
                label="English translation"
                fullWidth
                autoFocus
                className={classes.textField}
                value={english}
                onChange={(e) => setEnglish(e.target.value)}
                placeholder="Type the English translation..."
                onKeyDown={(e) => {
                  if (e.key === "Enter" && !disabled) {
                    e.preventDefault();
                    handleSave();
                  }
                }}
              />
            </>
          ) : (
            <>
              <TextField
                label="Hebrew"
                fullWidth
                autoFocus
                value={hebrew}
                className={classes.textField}
                onChange={(e) => setHebrew(e.target.value)}
                placeholder="הכנס מילה בעברית…"
                inputProps={{ dir: "rtl" }}
                sx={{ mb: 1.5 }}
              />
              <TextField
                label="English"
                fullWidth
                value={english}
                className={classes.textField}
                onChange={(e) => setEnglish(e.target.value)}
                placeholder="Enter the English translation..."
                onKeyDown={(e) => {
                  if (e.key === "Enter" && !disabled) {
                    e.preventDefault();
                    handleSave();
                  }
                }}
              />
            </>
          )}
        </Box>
      </DialogContent>

      <DialogActions className={classes.actions}>
        <Button
          onClick={onClose}
          variant="text"
          className={classes.cancelButton}
        >
          Cancel
        </Button>
        <Button
          onClick={handleSave}
          variant="contained"
          disabled={disabled}
          className={classes.saveButton}
        >
          {createCard.isPending ? "Saving…" : "Save"}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
