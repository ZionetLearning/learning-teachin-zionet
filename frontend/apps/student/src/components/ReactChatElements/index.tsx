import React, { useRef, useEffect } from "react";
import { MessageBox, Input } from "react-chat-elements";
import { useTranslation } from "react-i18next";
import type { ChatMessage } from "../../hooks";
import { useStyles } from "./style";
import avatar from "@student/assets/avatar.svg";
import "react-chat-elements/dist/main.css";

interface ReactChatElementsProps {
  messages: ChatMessage[] | undefined;
  loading: boolean;
  isPlaying?: boolean;
  avatarMode?: boolean;
  value: string;
  onChange: (value: string) => void;
  handleSendMessage: () => void;
  handlePlay?: () => void;
  handleStop?: () => void;
}

export const ReactChatElements = ({
  messages,
  loading,
  isPlaying = false,
  avatarMode = false,
  value,
  onChange,
  handleSendMessage,
  handlePlay,
  handleStop,
}: ReactChatElementsProps) => {
  const { t } = useTranslation();
  const classes = useStyles();
  const avatarUrl = avatar;
  const listRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const el = listRef.current;
    if (!el) return;

    const scrollToBottom = () => {
      el.scrollTo({
        top: el.scrollHeight,
        behavior: "smooth",
      });
    };

    scrollToBottom();

    const timeoutId = setTimeout(() => {
      el.scrollTo({
        top: el.scrollHeight,
        behavior: "auto",
      });
    }, 100);

    return () => clearTimeout(timeoutId);
  }, [messages]);

  return (
    <div className={classes.chatContainer}>
      <div
        className={
          avatarMode ? classes.messagesListAvatar : classes.messagesList
        }
        ref={listRef}
        data-testid="chat-yo-messages"
      >
        {messages?.map((msg, i) => {
          return (
            <div
              key={i}
              data-testid={
                msg.position === "right"
                  ? "chat-yo-msg-user"
                  : "chat-yo-msg-assistant"
              }
            >
              <MessageBox
                className={classes.messageBox}
                styles={{
                  backgroundColor:
                    msg.position === "right" ? "#11bbff" : "#fff",
                  color: "#000",
                }}
                id={String(i)}
                position={msg.position}
                type="text"
                text={
                  msg.isTyping && !msg.text
                    ? t("pages.chatYo.thinking")
                    : msg.text
                }
                title={msg.position === "right" ? "Me" : "Assistant"}
                titleColor={msg.position === "right" ? "black" : "gray"}
                date={msg.date}
                forwarded={false}
                replyButton={false}
                removeButton={false}
                status={msg.isTyping ? "waiting" : "received"}
                retracted={false}
                focus={false}
                avatar={msg.position === "left" ? avatarUrl : undefined}
                notch
              />
              {/* Show typing indicator for streaming messages with content */}
              {msg.isTyping && msg.text && (
                <div className={classes.typingIndicator}>
                  <span className={classes.typingDot}>●</span>
                  <span className={classes.typingDot}>●</span>
                  <span className={classes.typingDot}>●</span>
                </div>
              )}
            </div>
          );
        })}
      </div>

      <div
        className={avatarMode ? classes.inputContainer : undefined}
        data-testid="chat-yo-input-wrapper"
      >
        <Input
          data-testid="chat-yo-input"
          placeholder={t("pages.chatYo.typeMessage")}
          className={avatarMode ? classes.inputAvatar : classes.input}
          value={value}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
            onChange(e.target.value)
          }
          onKeyDown={(e) => e.key === "Enter" && handleSendMessage()}
          maxHeight={100}
          rightButtons={
            avatarMode ? (
              <div className={classes.rightButtons}>
                <button
                  className={classes.sendButton}
                  title={t("pages.chatYo.replayAvatar")}
                  onClick={() => {
                    if (isPlaying) {
                      handleStop?.();
                    } else {
                      handlePlay?.();
                    }
                  }}
                  data-testid="chat-yo-replay"
                >
                  {isPlaying ? (
                    <span style={{ fontSize: "15px" }}>■</span>
                  ) : (
                    "🗣"
                  )}
                </button>
                <button
                  className={classes.sendButton}
                  title={t("pages.chatYo.send")}
                  onClick={handleSendMessage}
                  data-testid="chat-yo-send"
                >
                  {loading ? "…" : "↑"}
                </button>
              </div>
            ) : (
              <button
                className={classes.sendButton}
                onClick={handleSendMessage}
                data-testid="chat-yo-send"
              >
                {loading ? "…" : "↑"}
              </button>
            )
          }
        />
      </div>
    </div>
  );
};
