import { useState, useRef, useEffect } from "react";
import {
  Drawer,
  Fab,
  IconButton,
  TextField,
  Typography,
  Box,
} from "@mui/material";
import ChatIcon from "@mui/icons-material/Chat";
import CloseIcon from "@mui/icons-material/Close";
import SendIcon from "@mui/icons-material/Send";
import { useTranslation } from "react-i18next";
import { useStyles } from "./style";
import type { ChatMessage, PageContext } from "./types";
import { useContextAwareChat } from "./useContextAwareChat";

export interface ContextAwareChatProps {
  pageContext: PageContext;
}

export const ContextAwareChat = ({ pageContext }: ContextAwareChatProps) => {
  const { t, i18n } = useTranslation();
  const isRTL = i18n.dir() === "rtl";
  const classes = useStyles({ isRTL });
  const [isOpen, setIsOpen] = useState(false);
  const [inputValue, setInputValue] = useState("");
  const messagesEndRef = useRef<HTMLDivElement>(null);

  const { messages, sendMessage, isLoading } = useContextAwareChat(pageContext);

  useEffect(
    function scrollToBottom() {
      messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
    },
    [messages],
  );

  const handleSend = () => {
    if (inputValue.trim() && !isLoading) {
      sendMessage(inputValue);
      setInputValue("");
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const renderContextInfo = () => {
    const parts: string[] = [];

    if (pageContext.exerciseType) {
      parts.push(t(`contextChat.exerciseType.${pageContext.exerciseType}`));
    }

    if (
      pageContext.currentExercise !== undefined &&
      pageContext.totalExercises !== undefined
    ) {
      parts.push(
        `${pageContext.currentExercise}/${pageContext.totalExercises}`,
      );
    }

    if (pageContext.difficulty) {
      parts.push(t(`contextChat.difficulty.${pageContext.difficulty}`));
    }

    return parts.length > 0 ? parts.join(" • ") : null;
  };

  return (
    <>
      <Fab
        color="primary"
        aria-label="chat"
        className={classes.floatingButton}
        onClick={() => setIsOpen(true)}
      >
        <ChatIcon />
      </Fab>

      <Drawer
        anchor={isRTL ? "left" : "right"}
        open={isOpen}
        onClose={() => setIsOpen(false)}
        className={classes.drawer}
      >
        <Box className={classes.chatContainer}>
          <Box className={classes.header}>
            <Typography variant="h6" className={classes.headerTitle}>
              {t("contextChat.title")}
            </Typography>
            <IconButton
              onClick={() => setIsOpen(false)}
              size="small"
              className={classes.closeButton}
            >
              <CloseIcon />
            </IconButton>
          </Box>

          <Box className={classes.contextInfo}>{renderContextInfo()}</Box>

          <Box className={classes.messagesContainer}>
            {messages.length === 0 ? (
              <Box className={classes.emptyState}>
                <ChatIcon className={classes.emptyStateIcon} />
                <Typography variant="body2" className={classes.emptyStateText}>
                  {t("contextChat.emptyState")}
                </Typography>
              </Box>
            ) : (
              messages.map((message: ChatMessage) => (
                <Box
                  key={message.id}
                  className={`${classes.message} ${
                    message.sender === "user"
                      ? classes.userMessage
                      : classes.assistantMessage
                  }`}
                >
                  {message.text}
                </Box>
              ))
            )}
            <div ref={messagesEndRef} />
          </Box>

          <Box className={classes.inputContainer}>
            <TextField
              className={classes.input}
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              onKeyDown={handleKeyDown}
              placeholder={t("contextChat.inputPlaceholder")}
              disabled={isLoading}
              size="small"
              multiline
              maxRows={3}
              variant="outlined"
            />
            <IconButton
              className={classes.sendButton}
              onClick={handleSend}
              disabled={!inputValue.trim() || isLoading}
            >
              <SendIcon />
            </IconButton>
          </Box>
        </Box>
      </Drawer>
    </>
  );
};

export type { PageContext, ChatMessage } from "./types";
