import React from "react";
import { useTranslation } from "react-i18next";
import { Chat } from "@student/types";
import { useStyles } from "./style";

interface ChatHistoryProps {
  allChats?: Chat[];
  isLoadingChats: boolean;
  isLoadingHistory: boolean;
  currentThreadId?: string;
  showSidebar: boolean;
  onChatSelect: (chatId: string) => void;
  onNewChat: () => void;
  onToggleSidebar: () => void;
  onCloseSidebar: () => void;
}

export const ChatHistory: React.FC<ChatHistoryProps> = ({
  allChats,
  isLoadingChats,
  isLoadingHistory,
  currentThreadId,
  showSidebar,
  onChatSelect,
  onNewChat,
  onToggleSidebar,
  onCloseSidebar,
}) => {
  const classes = useStyles();
  const { t } = useTranslation();

  const formatDate = (dateString: string) => {
    try {
      const date = new Date(dateString);
      const now = new Date();
      const diffTime = now.getTime() - date.getTime();
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

      if (diffDays === 1) return t("pages.chatHistory.dateFormat.today");
      if (diffDays === 2) return t("pages.chatHistory.dateFormat.yesterday");  
      if (diffDays <= 7) return t("pages.chatHistory.dateFormat.daysAgo", { count: diffDays });
      return date.toLocaleDateString();
    } catch {
      return t("pages.chatHistory.dateFormat.unknown");
    }
  };

  const truncateText = (text: string, maxLength: number = 35) => {
    return text.length > maxLength ? `${text.substring(0, maxLength)}...` : text;
  };

  return (
    <>
      {/* Sidebar Toggle Button */}
      <button
        onClick={onToggleSidebar}
        className={classes.sidebarToggle}
        title={t("pages.chatHistory.toggleButton.title")}
        aria-label={t("pages.chatHistory.toggleButton.ariaLabel")}
      >
        <span className={classes.hamburgerIcon}>
          <span></span>
          <span></span>
          <span></span>
        </span>
      </button>

      {/* Chat History Sidebar */}
      <div className={`${classes.sidebar} ${showSidebar ? classes.sidebarOpen : ''}`}>
        <div className={classes.sidebarHeader}>
          <div className={classes.headerTop}>
            <h3 className={classes.title}>{t("pages.chatHistory.title")}</h3>
            <button
              onClick={onCloseSidebar}
              className={classes.closeButton}
              title={t("pages.chatHistory.closeButton.title")}
              aria-label={t("pages.chatHistory.closeButton.ariaLabel")}
            >
              ×
            </button>
          </div>
          <button
            onClick={onNewChat}
            className={classes.newChatButton}
            title={t("pages.chatHistory.newChatButton.title")}
          >
            <span className={classes.plusIcon}>+</span>
            {t("pages.chatHistory.newChatButton.text")}
          </button>
        </div>

        <div className={classes.chatList}>
          {isLoadingChats ? (
            <div className={classes.loadingContainer}>
              <div className={classes.loadingSpinner}></div>
              <span className={classes.loadingText}>
                {t("pages.chatHistory.loading.chats")}
              </span>
            </div>
          ) : (
            <>
              {allChats && allChats.length > 0 ? (
                <div className={classes.chatItems}>
                  {allChats.map((chat) => (
                    <div
                      key={chat.chatId}
                      onClick={() => onChatSelect(chat.chatId)}
                      className={`${classes.chatItem} ${
                        currentThreadId === chat.chatId ? classes.activeChatItem : ''
                      }`}
                      role="button"
                      tabIndex={0}
                      onKeyDown={(e) => {
                        if (e.key === 'Enter' || e.key === ' ') {
                          onChatSelect(chat.chatId);
                        }
                      }}
                    >
                      <div className={classes.chatContent}>
                        <div className={classes.chatName}>
                          {truncateText(chat.chatName || t("pages.chatHistory.untitledChat"))}
                        </div>
                        <div className={classes.chatDate}>
                          {formatDate(chat.updatedAt)}
                        </div>
                      </div>
                      <div className={classes.chatIcon}>
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
                          <path d="M8 12h8M8 8h8M8 16h5M4 6v12c0 1.1.9 2 2 2h12c0-1.1-.9-2-2-2H6V6c0-1.1.9-2 2-2h12c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H6c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/>
                        </svg>
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <div className={classes.emptyState}>
                  <div className={classes.emptyIcon}>
                    <svg width="48" height="48" viewBox="0 0 24 24" fill="currentColor">
                      <path d="M20 2H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h4v6l4-6h8c1.1 0 2-.9 2-2V4c0-1.1-.9-2-2-2zm-2 12H6v-2h12v2zm0-3H6V9h12v2zm0-3H6V6h12v2z"/>
                    </svg>
                  </div>
                  <div className={classes.emptyTitle}>
                    {t("pages.chatHistory.emptyState.title")}
                  </div>
                  <div className={classes.emptyDescription}>
                    {t("pages.chatHistory.emptyState.description")}
                  </div>
                </div>
              )}
            </>
          )}
        </div>

        {isLoadingHistory && (
          <div className={classes.loadingHistory}>
            <div className={classes.loadingSpinnerSmall}></div>
            <span>{t("pages.chatHistory.loading.history")}</span>
          </div>
        )}
      </div>

      {/* Sidebar Overlay for mobile */}
      {showSidebar && (
        <div
          className={classes.sidebarOverlay}
          onClick={onCloseSidebar}
          role="button"
          tabIndex={0}
          onKeyDown={(e) => {
            if (e.key === 'Enter' || e.key === ' ') {
              onCloseSidebar();
            }
          }}
          aria-label={t("pages.chatHistory.overlay.ariaLabel")}
        />
      )}
    </>
  );
};
