import React, { useEffect, useState } from "react";
import { Box } from "@mui/material";
import { AddWordCardDialog } from "../../components";

type WordCaptureProviderProps = {
  children: React.ReactNode;
  minLength?: number;
  maxLength?: number;
};

export const WordCaptureProvider: React.FC<WordCaptureProviderProps> = ({
  children,
  minLength = 1,
  maxLength = 20,
}) => {
  const [open, setOpen] = useState(false);
  const [hebrewWord, setHebrewWord] = useState<string>("");

  // Detect a single Hebrew "word" in a selection.
  const extractHebrewWord = (raw: string): string | null => {
    const trimmed = raw.trim();
    if (trimmed.length < minLength || trimmed.length > maxLength) return null;

    // Hebrew block + maqaf + geresh/gershayim
    const match = trimmed.match(/[\u0590-\u05FF\u05BE\u05F3\u05F4]+/);
    if (!match) return null;

    const word = match[0];
    if (/\s/.test(trimmed)) return null; // no multi-word
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
  };

  return (
    <Box>
      {children}
      <AddWordCardDialog
        open={open}
        onClose={handleClose}
        initialHebrew={hebrewWord}
      />
    </Box>
  );
};
