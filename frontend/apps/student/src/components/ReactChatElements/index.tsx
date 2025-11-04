import React, { useRef, useEffect } from "react";
import { MessageBox, Input } from "react-chat-elements";
import { useTranslation } from "react-i18next";
import type { ChatMessage } from "../../hooks";
import { renderWithBold } from "@student/utils";
import avatar from "@student/assets/avatar.svg";
import "react-chat-elements/dist/main.css";
import { useStylesWithMode } from "./style";

interface ReactChatElementsProps {
  messages: ChatMessage[] | undefined;
  loading: boolean;
  value: string;
  onChange: (value: string) => void;
  handleSendMessage: () => void;
}

export const ReactChatElements = ({
  messages,
  loading,
  value,
  onChange,
  handleSendMessage,
}: ReactChatElementsProps) => {
  const { t } = useTranslation();
  const classes = useStylesWithMode();
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
        className={classes.messagesList}
        ref={listRef}
        data-testid="chat-yo-messages"
      >
        {messages?.map((msg, i) => (
          <div
            key={i}
            data-testid={
              msg.position === "right"
                ? "chat-yo-msg-user"
                : "chat-yo-msg-assistant"
            }
          >
            <MessageBox
              className={`${classes.messageBox} ${
                msg.position === "right"
                  ? classes.bubbleRight
                  : classes.bubbleLeft
              }`}
              id={String(i)}
              position={msg.position}
              type="text"
              text={
                (msg.isTyping && !msg.text
                  ? t("pages.chatYo.thinking")
                  : renderWithBold(msg.text)) as unknown as string
              }
              title={
                msg.position === "right"
                  ? t("pages.chatAvatar.me")
                  : t("pages.chatAvatar.avatar")
              }
              titleColor="inherit"
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
          </div>
        ))}
      </div>

      <div
        className={classes.inputContainer}
        data-testid="chat-yo-input-wrapper"
      >
        <Input
          data-testid="chat-yo-input"
          placeholder={t("pages.chatYo.typeMessage")}
          className={classes.input}
          value={value}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
            onChange(e.target.value)
          }
          onKeyDown={(e) => e.key === "Enter" && handleSendMessage()}
          maxHeight={120}
          rightButtons={
            <button
              className={classes.sendButton}
              title={t("pages.chatYo.send")}
              onClick={handleSendMessage}
              data-testid="chat-yo-send"
              disabled={loading || !value.trim()}
            >
              {loading ? "…" : t("pages.chatAvatar.send")}
            </button>
          }
        />
      </div>
    </div>
  );
};
