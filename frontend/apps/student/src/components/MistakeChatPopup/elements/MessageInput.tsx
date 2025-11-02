import React, { useState } from "react";
import { Box } from "@mui/material";
import { useTranslation } from "react-i18next";
import { useStyles } from "../style";

interface MessageInputProps {
  onSendMessage: (message: string) => void;
  disabled?: boolean;
  placeholder?: string;
  isRTL: boolean;
}

export const MessageInput = ({
  onSendMessage,
  disabled = false,
  placeholder,
  isRTL,
}: MessageInputProps) => {
  const [input, setInput] = useState("");
  const { t } = useTranslation();
  const classes = useStyles({ isRTL });

  const placeholderText = placeholder || t("mistakeChat.inputPlaceholder", {
    defaultValue: "Type your message...",
  });

  const handleSend = () => {
    const trimmed = input.trim();
    if (trimmed && !disabled) {
      onSendMessage(trimmed);
      setInput("");
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  return (
    <Box className={classes.inputWrapper}>
      <textarea
        value={input}
        onChange={(e) => setInput(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder={placeholderText}
        disabled={disabled}
        className={classes.input}
        rows={1}
      />
      <button
        onClick={handleSend}
        disabled={disabled || !input.trim()}
        className={classes.sendButton}
      >
        {t("mistakeChat.send", { defaultValue: "Send" })}
      </button>
    </Box>
  );
};
