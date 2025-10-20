// WordCaptureProvider.tsx
import React, { useEffect, useRef, useState } from "react";
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
  const rootRef = useRef<HTMLDivElement | null>(null);

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
        // Clear selection so it doesn't stay highlighted
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
    <Box ref={rootRef} sx={{ display: "contents" }}>
      {children}

      <Dialog open={open} onClose={handleClose} maxWidth="xs" fullWidth>
        <DialogTitle sx={{ pr: 6 }}>
          Add Translation
          <IconButton
            onClick={handleClose}
            aria-label="close"
            sx={{ position: "absolute", right: 8, top: 8 }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>

        <DialogContent>
          <Box
            sx={{
              mb: 2,
              p: 1.5,
              borderRadius: 2,
              bgcolor: "background.default",
              border: (theme) => `1px solid ${theme.palette.divider}`,
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
              gap: 2,
            }}
          >
            <Box>
              <Typography variant="caption" color="text.secondary">
                Hebrew word
              </Typography>
              <Typography
                variant="h6"
                sx={{ direction: "rtl", fontWeight: 700, lineHeight: 1.3 }}
              >
                {hebrewWord}
              </Typography>
            </Box>
          </Box>

          <TextField
            label="English translation"
            fullWidth
            autoFocus
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
        </DialogContent>

        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={handleClose} variant="text">
            Cancel
          </Button>
          <Button
            onClick={handleSave}
            variant="contained"
            disabled={saveDisabled}
          >
            {createWordCard.isPending ? "Saving…" : "Save"}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};
