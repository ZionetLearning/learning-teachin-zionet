import React, { useEffect, useRef, useState, useCallback } from "react";
import type { Message } from "../../types/Message";
import { useStyles } from "./style";
import { MessageItem } from "../MessageItem";

interface MessageListProps {
  messages: Message[];
  isLoading?: boolean;
}

const MessageList: React.FC<MessageListProps> = ({
  messages,
  isLoading = false,
}) => {
  const classes = useStyles();
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const [isUserScrolling, setIsUserScrolling] = useState(false);
  const [showScrollToBottom, setShowScrollToBottom] = useState(false);

  // Check if user is near the bottom of the scroll area
  const isNearBottom = useCallback(() => {
    if (!containerRef.current) return true;

    const { scrollTop, scrollHeight, clientHeight } = containerRef.current;
    const threshold = 100; // pixels from bottom
    return scrollHeight - scrollTop - clientHeight < threshold;
  }, []);

  // Auto-scroll to bottom when new messages arrive (only if user is near bottom)
  const scrollToBottom = useCallback(
    (force = false) => {
      if (force || (!isUserScrolling && isNearBottom())) {
        messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
        setShowScrollToBottom(false);
      }
    },
    [isUserScrolling, isNearBottom]
  );

  // Handle scroll events to detect user scrolling
  const scrollTimeoutRef = useRef<number | null>(null);

  const handleScroll = useCallback(() => {
    if (!containerRef.current) return;

    const isAtBottom = isNearBottom();
    setShowScrollToBottom(!isAtBottom);

    // Set user scrolling flag and clear any existing timeout
    setIsUserScrolling(true);

    if (scrollTimeoutRef.current) {
      clearTimeout(scrollTimeoutRef.current);
    }

    // Reset user scrolling flag after a longer delay (5 seconds)
    scrollTimeoutRef.current = setTimeout(() => {
      setIsUserScrolling(false);
    }, 5000);
  }, [isNearBottom]);

  // Auto-scroll when new messages arrive (only if user is not actively scrolling)
  useEffect(() => {
    // Only auto-scroll if user is not currently scrolling and is near bottom
    if (!isUserScrolling && isNearBottom()) {
      scrollToBottom();
    }
  }, [messages, scrollToBottom, isUserScrolling, isNearBottom]);

  // Cleanup timeout on unmount
  useEffect(() => {
    return () => {
      if (scrollTimeoutRef.current) {
        clearTimeout(scrollTimeoutRef.current);
      }
    };
  }, []);

  // Group messages by date for better organization
  const groupMessagesByDate = (messages: Message[]) => {
    const groups: { [key: string]: Message[] } = {};

    messages.forEach((message) => {
      const dateKey = message.timestamp.toDateString();
      if (!groups[dateKey]) {
        groups[dateKey] = [];
      }
      groups[dateKey].push(message);
    });

    return groups;
  };

  const messageGroups = groupMessagesByDate(messages);
  const today = new Date().toDateString();
  const yesterday = new Date(Date.now() - 24 * 60 * 60 * 1000).toDateString();

  const formatDateHeader = (dateString: string) => {
    if (dateString === today) return "Today";
    if (dateString === yesterday) return "Yesterday";
    return new Date(dateString).toLocaleDateString([], {
      weekday: "long",
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  };

  return (
    <div className={classes.container} ref={containerRef}>
      <div className={classes.messagesList} onScroll={handleScroll}>
        {Object.keys(messageGroups).length === 0 && !isLoading ? (
          <div className={classes.emptyState}>
            <div className={classes.emptyStateIcon}>💬</div>
            <div className={classes.emptyStateText}>
              Start a conversation by sending a message below
            </div>
          </div>
        ) : (
          Object.entries(messageGroups).map(([dateString, groupMessages]) => (
            <div key={dateString} className={classes.messageGroup}>
              <div className={classes.dateHeader}>
                {formatDateHeader(dateString)}
              </div>
              {groupMessages.map((message, index) => {
                const prevMessage = index > 0 ? groupMessages[index - 1] : null;
                const isConsecutive =
                  prevMessage &&
                  prevMessage.sender.id === message.sender.id &&
                  message.timestamp.getTime() -
                    prevMessage.timestamp.getTime() <
                    5 * 60 * 1000; // 5 minutes

                return (
                  <div
                    key={message.id}
                    className={`${classes.messageWrapper} ${isConsecutive ? classes.consecutiveMessage : ""}`}
                  >
                    <MessageItem message={message} />
                  </div>
                );
              })}
            </div>
          ))
        )}

        {isLoading && (
          <div className={classes.loadingIndicator}>
            <div className={classes.typingDots}>
              <span></span>
              <span></span>
              <span></span>
            </div>
            <span className={classes.typingText}>AI is typing...</span>
          </div>
        )}

        <div ref={messagesEndRef} />
      </div>

      {/* Scroll to bottom button */}
      {showScrollToBottom && (
        <button
          className={classes.scrollToBottomButton}
          onClick={() => scrollToBottom(true)}
          aria-label="Scroll to bottom"
          title="Scroll to latest messages"
        >
          ↓
        </button>
      )}
    </div>
  );
};

export { MessageList };
