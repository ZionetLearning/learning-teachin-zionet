import React, { useEffect, useState, useCallback } from "react";
import { useTranslation } from "react-i18next";
import { ChatHeader, MessageList, MessageInput } from "./components";
import { useChat } from "./hooks";
import { useStyles } from "./style";
import type { MessageContext } from "./types";

export const ChatOu: React.FC = () => {
  const { t } = useTranslation();
  const classes = useStyles();
  const {
    messages,
    isLoading,
    error,
    sendMessage,
    retryLastMessage,
    isInitialized,
  } = useChat();

  const [showError, setShowError] = useState(false);

  //  forcing an error to test Error Boundary of sentry
  //  useEffect(() => {
  //   throw new Error("Test error for Sentry Error Boundary");
  // }, []);

  useEffect(() => {
    if (error) {
      setShowError(true);
    }
  }, [error]);

  const handleSendMessage = useCallback(
    async (content: string, context?: MessageContext) => {
      try {
        await sendMessage(content, context);
      } catch (err) {
        console.error("Failed to send message:", err);
        // Error is already handled by useChat hook
      }
    },
    [sendMessage],
  );

  const handleDismissError = useCallback(() => {
    setShowError(false);
  }, []);

  const handleRetryLastAction = useCallback(async () => {
    try {
      await retryLastMessage();
      setShowError(false);
    } catch (err) {
      console.error("Failed to retry message:", err);
      // Error will be handled by useChat hook
    }
  }, [retryLastMessage]);

  // Loading state for initial load
  if (!isInitialized && messages.length === 0) {
    return (
      <div className={classes.container}>
        <div className={classes.loadingOverlay}>
          <div className={classes.loadingSpinner} />
        </div>
      </div>
    );
  }

  return (
    <div className={classes.container}>
      {/* Error Display */}
      {showError && error && (
        <div className={classes.errorContainer}>
          <span className={classes.errorMessage}>{error}</span>
          <div>
            <button
              className={classes.errorDismiss}
              onClick={handleRetryLastAction}
              aria-label="Retry action"
              style={{ marginRight: "8px" }}
            >
              {t('pages.chatOu.retry')}
            </button>
            <button
              className={classes.errorDismiss}
              onClick={handleDismissError}
              aria-label="Dismiss error"
            >
              ×
            </button>
          </div>
        </div>
      )}

      {/* Chat Header */}
      <div className={classes.header}>
        <ChatHeader
          title={t('pages.chatOu.smartChatDemo')}
          isOnline={!error}
          isTyping={isLoading}
          participantCount={2}
        />
      </div>

      {/* Message Area */}
      <div className={classes.messageArea}>
        <div className={classes.messageList}>
          <MessageList messages={messages} isLoading={isLoading} />
        </div>

        <div className={classes.inputArea}>
          <MessageInput
            onSendMessage={handleSendMessage}
            isLoading={isLoading}
            placeholder={t('pages.chatOu.typeYourMessage')}
            disabled={!!error}
          />
        </div>
      </div>

      {/* Loading Overlay - Only show for major operations */}
      {isLoading && messages.length === 0 && (
        <div className={classes.loadingOverlay}>
          <div className={classes.loadingSpinner} />
        </div>
      )}
    </div>
  );
};