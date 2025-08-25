import React from "react";
import { useTranslation } from "react-i18next";
import type { Message } from "../../types";
import { useStyles } from "./style";
import {
  ImageMessage,
  TextMessage,
  GenerativeUIMessage,
} from "../MessageRenderers";

interface MessageItemProps {
  message: Message;
}

const MessageItem: React.FC<MessageItemProps> = ({ message }) => {
  const { t } = useTranslation();
  const classes = useStyles();

  const formatTimestamp = (timestamp: Date) => {
    return timestamp.toLocaleTimeString([], {
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const renderMessageContent = () => {
    switch (message.type) {
      case "text":
        return <TextMessage message={message} />;
      case "image":
        return <ImageMessage message={message} />;
      case "generative-ui":
        return <GenerativeUIMessage message={message} />;
      default:
        return (
          <div className={classes.fallback}>
            {t("pages.chatOu.unsupportedMessageType")}
          </div>
        );
    }
  };

  const isUserMessage = message.sender.type === "user";

  return (
    <div
      className={`${classes.container} ${isUserMessage ? classes.userMessage : classes.aiMessage}`}
    >
      <div className={classes.messageHeader}>
        <div className={classes.senderInfo}>
          {message.sender.avatar && (
            <div className={classes.avatar}>{message.sender.avatar}</div>
          )}
          <span className={classes.senderName}>{message.sender.name}</span>
        </div>
        <span className={classes.timestamp}>
          {formatTimestamp(message.timestamp)}
        </span>
      </div>

      <div className={classes.messageContent}>{renderMessageContent()}</div>

      {message.context && (
        <div className={classes.contextInfo}>
          <span className={classes.contextLabel}>
            {t("pages.chatOu.context")}
          </span>
          {message.context.pageTitle && (
            <span className={classes.contextItem}>
              {t("pages.chatOu.page")} {message.context.pageTitle}
            </span>
          )}
          {message.context.selectedText && (
            <span className={classes.contextItem}>
              {t("pages.chatOu.selected")} "
              {message.context.selectedText.substring(0, 50)}..."
            </span>
          )}
        </div>
      )}
    </div>
  );
};

export { MessageItem };
