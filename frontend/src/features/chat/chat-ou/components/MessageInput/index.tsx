import React, { useState, useRef, type KeyboardEvent } from "react";
import { useStyles } from "./style";
import { useContext } from "../../hooks/useContext";
import type { MessageContext } from "../../types";

const DEMO_SUGGESTIONS = [
  "Show me an image",
  "Create a quiz for me",
  "Give me a navigation link",
  "What can you do?",
];

interface MessageInputProps {
  onSendMessage: (content: string, context?: MessageContext) => Promise<void>;
  isLoading?: boolean;
  placeholder?: string;
  disabled?: boolean;
}

export const MessageInput: React.FC<MessageInputProps> = ({
  onSendMessage,
  isLoading = false,
  placeholder = "Type your message...",
  disabled = false,
}) => {
  const classes = useStyles();
  const [inputValue, setInputValue] = useState("");
  const [isSending, setIsSending] = useState(false);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  const {
    currentContext,
    isContextAttached,
    attachContext,
    detachContext,
    refreshContext,
    hasSignificantContext,
    contextDisplayText,
  } = useContext();

  const handleInputChange = (event: React.ChangeEvent<HTMLTextAreaElement>) => {
    setInputValue(event.target.value);

    // Auto-resize textarea
    if (textareaRef.current) {
      textareaRef.current.style.height = "auto";
      textareaRef.current.style.height = `${textareaRef.current.scrollHeight}px`;
    }
  };

  const handleKeyDown = (event: KeyboardEvent<HTMLTextAreaElement>) => {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      handleSendMessage();
    }
  };

  const handleSendMessage = async () => {
    const trimmedValue = inputValue.trim();

    // Validation
    if (!trimmedValue || isSending || disabled || isLoading) {
      return;
    }

    setIsSending(true);

    try {
      // Send message with context if attached
      const contextToSend =
        isContextAttached && currentContext ? currentContext : undefined;
      await onSendMessage(trimmedValue, contextToSend);
      setInputValue("");

      // Reset textarea height
      if (textareaRef.current) {
        textareaRef.current.style.height = "auto";
      }

      // Optionally detach context after sending
      if (isContextAttached) {
        detachContext();
      }
    } catch (error) {
      console.error("Failed to send message:", error);
    } finally {
      setIsSending(false);
    }
  };

  const handleContextToggle = () => {
    if (isContextAttached) {
      detachContext();
    } else {
      attachContext();
    }
  };

  const handleRefreshContext = () => {
    refreshContext();
  };

  const handleSuggestionClick = (suggestion: string) => {
    setInputValue(suggestion);
    setShowSuggestions(false);

    // Auto-resize textarea
    if (textareaRef.current) {
      textareaRef.current.style.height = "auto";
      textareaRef.current.style.height = `${textareaRef.current.scrollHeight}px`;
    }

    // Focus the textarea
    textareaRef.current?.focus();
  };

  const handleInputFocus = () => {
    if (!inputValue.trim()) {
      setShowSuggestions(true);
    }
  };

  const handleInputBlur = () => {
    // Delay hiding suggestions to allow clicking on them
    setTimeout(() => setShowSuggestions(false), 150);
  };

  const isButtonDisabled =
    !inputValue.trim() || isSending || disabled || isLoading;

  return (
    <div className={classes.container}>
      {/* Context Display */}
      {isContextAttached && currentContext && (
        <div className={classes.contextDisplay}>
          <div className={classes.contextHeader}>
            <span className={classes.contextIcon}>📎</span>
            <span className={classes.contextLabel}>Context Attached</span>
            <div className={classes.contextActions}>
              <button
                className={classes.contextActionButton}
                onClick={handleRefreshContext}
                title="Refresh context"
                type="button"
              >
                🔄
              </button>
              <button
                className={classes.contextActionButton}
                onClick={detachContext}
                title="Remove context"
                type="button"
              >
                ✕
              </button>
            </div>
          </div>
          {hasSignificantContext && (
            <div className={classes.contextContent}>{contextDisplayText}</div>
          )}
        </div>
      )}

      <div className={classes.inputWrapper}>
        {/* Context Toggle Button */}
        <button
          className={`${classes.contextButton} ${isContextAttached ? classes.contextButtonActive : ""}`}
          onClick={handleContextToggle}
          disabled={disabled || isLoading}
          type="button"
          title={isContextAttached ? "Remove context" : "Attach page context"}
          aria-label={
            isContextAttached ? "Remove context" : "Attach page context"
          }
        >
          📎
        </button>

        <textarea
          ref={textareaRef}
          className={classes.textarea}
          value={inputValue}
          onChange={handleInputChange}
          onKeyDown={handleKeyDown}
          onFocus={handleInputFocus}
          onBlur={handleInputBlur}
          placeholder={placeholder}
          disabled={disabled || isLoading}
          rows={1}
          maxLength={2000}
        />
        <button
          className={`${classes.sendButton} ${isButtonDisabled ? classes.sendButtonDisabled : ""}`}
          onClick={handleSendMessage}
          disabled={isButtonDisabled}
          type="button"
          aria-label="Send message"
        >
          {isSending ? (
            <span className={classes.loadingSpinner}>⏳</span>
          ) : (
            <span className={classes.sendIcon}>➤</span>
          )}
        </button>
      </div>
      {/* Demo Suggestions */}
      {showSuggestions && !inputValue.trim() && (
        <div className={classes.suggestionsPanel}>
          <div className={classes.suggestionsHeader}>
            💡 Try these demo examples:
          </div>
          <div className={classes.suggestionsList}>
            {DEMO_SUGGESTIONS.map((suggestion, index) => (
              <button
                key={index}
                className={classes.suggestionButton}
                onClick={() => handleSuggestionClick(suggestion)}
                type="button"
              >
                {suggestion}
              </button>
            ))}
          </div>
        </div>
      )}

      {inputValue.length > 1800 && (
        <div className={classes.characterCount}>
          {inputValue.length}/2000 characters
        </div>
      )}
    </div>
  );
};
