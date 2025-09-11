import React from "react";
import { IconButton } from "@mui/material";
import ChatBubbleOutlineIcon from "@mui/icons-material/ChatBubbleOutline";
import MenuIcon from "@mui/icons-material/Menu";
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
      const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24));

      if (diffDays < 1) return t("pages.chatHistory.dateFormat.today");
      if (diffDays === 1) return t("pages.chatHistory.dateFormat.yesterday");  
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
      {/* Sidebar Toggle Button - only show when sidebar is closed */}
      {!showSidebar && (
        <IconButton
          onClick={onToggleSidebar}
          className={classes.sidebarToggle}
          title={t("pages.chatHistory.toggleButton.title")}
          aria-label={t("pages.chatHistory.toggleButton.ariaLabel")}
        >
          <MenuIcon />
        </IconButton>
      )}

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
                        <ChatBubbleOutlineIcon fontSize="small" />
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <div className={classes.emptyState}>
                  <div className={classes.emptyIcon}>
                     <div className={classes.emptyTitle}>
                    <ChatBubbleOutlineIcon fontSize="small" />
                    {t("pages.chatHistory.emptyState.title")}
                  </div>
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
