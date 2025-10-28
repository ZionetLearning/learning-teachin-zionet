import { useState, useEffect, useRef } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  IconButton,
  Typography,
  Box,
  CircularProgress,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import { useTranslation } from "react-i18next";
import { useMistakeChat } from "@student/hooks";
import { useStyles } from "./style";
import { MessageInput } from "./elements";

export interface MistakeChatPopupProps {
  open: boolean;
  onClose: () => void;
  attemptId: string;
  gameType: string;
  title?: string;
}

export const MistakeChatPopup = ({
  open,
  onClose,
  attemptId,
  gameType,
  title = "Explain Mistake",
}: MistakeChatPopupProps) => {
  const { t, i18n } = useTranslation();
  const isRTL = i18n.dir() === "rtl";
  const classes = useStyles({ isRTL });
  const [threadId] = useState<string>(() => crypto.randomUUID());
  const bottomRef = useRef<HTMLDivElement>(null);

  const { messages, sendMessage, initialize, reset, loading, hasInitialized } =
    useMistakeChat({
      attemptId,
      threadId,
      gameType,
    });


  useEffect(() => {
    if (messages.length > 0) {
      bottomRef.current?.scrollIntoView({ behavior: "smooth" });
    }
  }, [messages]);

  useEffect(() => {
    if (open && !hasInitialized) {
      initialize();
    }
  }, [open, hasInitialized, initialize]);

  const handleClose = () => {
    reset();
    onClose();
  };

  const botTyping = messages.some((m) => m.sender === "bot" && m.isTyping);

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      className={classes.dialog}
      maxWidth={false}
      scroll="paper"
      slotProps={{
      transition: {
        timeout: 300,
      },
      }}
    >
      <DialogTitle className={classes.header}>
      <Typography variant="h6" component="div" className={classes.title}>
        {t("mistakeChat.title", { defaultValue: title })}
      </Typography>
      <IconButton
        aria-label="close"
        onClick={handleClose}
        className={classes.closeButton}
      >
        <CloseIcon />
      </IconButton>
      </DialogTitle>

      <DialogContent className={classes.content}>
      <Box className={classes.messagesContainer}>
        {messages.map((message, index) => (
        <Box
          key={index}
          className={`${classes.message} ${
          message.sender === "user"
            ? classes.userMessage
            : classes.botMessage
          }`}
        >
          <Typography variant="body1" className={classes.messageText}>
          {message.text}
          </Typography>
          {message.sender === "bot" && message.isTyping && (
          <Box className={classes.typingIndicator}>
            <CircularProgress size={12} />
          </Box>
          )}
        </Box>
        ))}

        {loading && messages.length === 0 && (
        <Box className={classes.loadingContainer}>
          <CircularProgress size={24} />
          <Typography variant="body2" className={classes.loadingText}>
          {t("mistakeChat.loadingExplanation", {
            defaultValue: "Loading explanation...",
          })}
          </Typography>
        </Box>
        )}

        <div ref={bottomRef} />
      </Box>

      {/* Simple message input for follow-up questions */}
      <Box className={classes.inputContainer}>
        <MessageInput
        onSendMessage={sendMessage}
        disabled={loading || botTyping}
        placeholder={t("mistakeChat.askFollowUp", {
          defaultValue: "Ask a follow-up question...",
        })}
        isRTL={isRTL}
        />
      </Box>
      </DialogContent>
    </Dialog>
  );
};
