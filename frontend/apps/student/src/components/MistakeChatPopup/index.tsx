import React, { useState, useEffect, useRef, useCallback } from "react";
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
import { useAuth } from "@app-providers";
import { EventType } from "@app-providers/types";
import {
  useSendChatMessageStream,
  useSendMistakeExplanation,
  ChatType,
} from "@student/api";
import { useSignalR } from "@student/hooks";
import { useStyles } from "./style";
import { AIChatStreamResponse } from "@student/types";

export interface MistakeChatMessage {
  id: string;
  text: string;
  sender: "user" | "bot";
  isComplete?: boolean;
}

export interface MistakeChatPopupProps {
  open: boolean;
  onClose: () => void;
  attemptId: string;
  gameType: string;
  title?: string;
}

export const MistakeChatPopup: React.FC<MistakeChatPopupProps> = ({
  open,
  onClose,
  attemptId,
  gameType,
  title = "Explain Mistake",
}) => {
  const { t, i18n } = useTranslation();
  const isRTL = i18n.dir() === "rtl";
  const classes = useStyles({ isRTL });
  const { user } = useAuth();
  const { sendMistakeExplanation } = useSendMistakeExplanation();
  const { startStream } = useSendChatMessageStream();
  const { waitForStream } = useSignalR();

  const [messages, setMessages] = useState<MistakeChatMessage[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [threadId] = useState<string>(() => crypto.randomUUID());
  const [hasRequestedExplanation, setHasRequestedExplanation] = useState(false);
  const bottomRef = useRef<HTMLDivElement>(null);

  // Auto-scroll to bottom when new messages arrive
  useEffect(() => {
    if (messages.length > 0) {
      bottomRef.current?.scrollIntoView({ behavior: "smooth" });
    }
  }, [messages]);

  // Request initial mistake explanation when popup opens
  const requestMistakeExplanation = useCallback(async () => {
    if (!open || !user?.userId || hasRequestedExplanation || isLoading) {
      return;
    }

    setIsLoading(true);
    setHasRequestedExplanation(true);

    let assistantBuffer = "";
    let currentBotMessageId: string | null = null;

    try {
      // Send the mistake explanation request
      const { requestId } = await sendMistakeExplanation({
        attemptId,
        threadId,
        gameType,
        chatType: ChatType.ExplainMistake,
      });

      // Listen for the streamed response using waitForStream
      await waitForStream<AIChatStreamResponse>(
        EventType.ChatAiAnswer,
        requestId,
        (msg) => {
          const payload = msg?.payload ?? {};
          const isFinal = !!payload?.isFinal;
          const delta = payload?.delta;

          // If final, mark message as complete
          if (isFinal) {
            if (currentBotMessageId) {
              setMessages((prev) =>
                prev.map((m) =>
                  m.id === currentBotMessageId ? { ...m, isComplete: true } : m
                )
              );
            }
            setIsLoading(false);
            return;
          }

          // Accumulate delta chunks
          if (typeof delta === "string" && delta.length > 0) {
            assistantBuffer += delta;

            if (!currentBotMessageId) {
              currentBotMessageId = "bot-" + Date.now();
              setMessages((prev) => [
                ...prev,
                {
                  id: currentBotMessageId!,
                  text: assistantBuffer,
                  sender: "bot",
                  isComplete: false,
                },
              ]);
            } else {
              setMessages((prev) =>
                prev.map((m) =>
                  m.id === currentBotMessageId
                    ? { ...m, text: assistantBuffer }
                    : m
                )
              );
            }
          }
        }
      );
    } catch (error) {
      console.error("Failed to request mistake explanation:", error);
      setMessages([
        {
          id: "error-" + Date.now(),
          text: t("mistakeChat.errorLoadingExplanation", {
            defaultValue: "Sorry, I couldn't load the explanation. Please try again.",
          }),
          sender: "bot",
          isComplete: true,
        },
      ]);
      setIsLoading(false);
    }
  }, [
    open,
    user?.userId,
    hasRequestedExplanation,
    isLoading,
    sendMistakeExplanation,
    attemptId,
    threadId,
    gameType,
    waitForStream,
    t,
  ]);

  // Request explanation when dialog opens
  useEffect(() => {
    if (open && !hasRequestedExplanation) {
      requestMistakeExplanation();
    }
  }, [open, requestMistakeExplanation, hasRequestedExplanation]);

  // Reset state when dialog closes
  const handleClose = () => {
    setMessages([]);
    setHasRequestedExplanation(false);
    setIsLoading(false);
    onClose();
  };

  // Send additional message (for follow-up questions)
  const sendMessage = useCallback(
    async (messageText: string) => {
      if (!user?.userId || !messageText.trim()) return;

      // Add user message
      const userMessage: MistakeChatMessage = {
        id: "user-" + Date.now(),
        text: messageText.trim(),
        sender: "user",
        isComplete: true,
      };

      setMessages((prev) => [...prev, userMessage]);
      setIsLoading(true);

      let assistantBuffer = "";
      let currentBotMessageId: string | null = null;

      try {
        // Use the normal chat stream for follow-up questions
        await startStream(
          {
            userMessage: messageText.trim(),
            threadId,
            chatType: ChatType.ExplainMistake, // Use the ChatType constant
            userId: user.userId,
          },
          (delta: string) => {
            assistantBuffer += delta;

            if (!currentBotMessageId) {
              currentBotMessageId = "bot-" + Date.now();
              setMessages((prev) => [
                ...prev,
                {
                  id: currentBotMessageId!,
                  text: assistantBuffer,
                  sender: "bot",
                  isComplete: false,
                },
              ]);
            } else {
              setMessages((prev) =>
                prev.map((msg) =>
                  msg.id === currentBotMessageId
                    ? { ...msg, text: assistantBuffer }
                    : msg
                )
              );
            }
          },
          () => {
            if (currentBotMessageId) {
              setMessages((prev) =>
                prev.map((msg) =>
                  msg.id === currentBotMessageId
                    ? { ...msg, isComplete: true }
                    : msg
                )
              );
            }
            setIsLoading(false);
          }
        );
      } catch (error) {
        console.error("Failed to send message:", error);
        setIsLoading(false);
      }
    },
    [user?.userId, threadId, startStream]
  );

  const botTyping = messages.some((m) => m.sender === "bot" && !m.isComplete);

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      className={classes.dialog}
      maxWidth={false}
      scroll="paper"
      TransitionProps={{
        timeout: 300,
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
          {messages.map((message) => (
            <Box
              key={message.id}
              className={`${classes.message} ${
                message.sender === "user"
                  ? classes.userMessage
                  : classes.botMessage
              }`}
            >
              <Typography variant="body1" className={classes.messageText}>
                {message.text}
              </Typography>
              {message.sender === "bot" && !message.isComplete && (
                <Box className={classes.typingIndicator}>
                  <CircularProgress size={12} />
                </Box>
              )}
            </Box>
          ))}

          {isLoading && messages.length === 0 && (
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
            disabled={isLoading || botTyping}
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

// Simple message input component
interface MessageInputProps {
  onSendMessage: (message: string) => void;
  disabled?: boolean;
  placeholder?: string;
  isRTL: boolean;
}

const MessageInput: React.FC<MessageInputProps> = ({
  onSendMessage,
  disabled = false,
  placeholder = "Type your message...",
  isRTL,
}) => {
  const [input, setInput] = useState("");
  const { t } = useTranslation();
  const classes = useStyles({ isRTL });

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
        placeholder={placeholder}
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