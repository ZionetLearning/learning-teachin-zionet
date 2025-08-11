import React, { useRef, useEffect } from "react";
import { MessageBox, Input } from "react-chat-elements";
import { useTranslation } from "react-i18next";
import type { ChatMessage } from "../../hooks";
import { useStyles } from "./style";
import avatar from "@/assets/avatar.svg";
import "react-chat-elements/dist/main.css";

interface ChatUiProps {
  messages: ChatMessage[] | undefined;
  loading: boolean;
  avatarMode?: boolean;
  value: string;
  onChange: (value: string) => void;
  handleSendMessage: () => void;
  handlePlay?: () => void;
}

export const ChatUi = ({
  messages,
  loading,
  avatarMode = false,
  value,
  onChange,
  handleSendMessage,
  handlePlay,
}: ChatUiProps) => {
  const { t } = useTranslation();
  const classes = useStyles();
  const avatarUrl = avatar;
  const listRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const el = listRef.current;
    if (!el) return;
    // scroll to the bottom
    el.scrollTop = el.scrollHeight;
  }, [messages]);

  return (
    <>
      <div
        className={
          avatarMode ? classes.messagesListAvatar : classes.messagesList
        }
        ref={listRef}
      >
        {messages?.map((msg, i) => (
          <MessageBox
            key={i}
            className={classes.messageBox}
            styles={{
              backgroundColor: msg.position === "right" ? "#11bbff" : "#fff",
              color: "#000",
            }}
            id={String(i)}
            position={msg.position}
            type="text"
            text={msg.text}
            title={msg.position === "right" ? "Me" : "Assistant"}
            titleColor={msg.position === "right" ? "black" : "gray"}
            date={msg.date}
            forwarded={false}
            replyButton={false}
            removeButton={false}
            status="received"
            retracted={false}
            focus={false}
            avatar={msg.position === "left" ? avatarUrl : undefined}
            notch
          />
        ))}
        {loading && (
          <MessageBox
            id="assistant"
            position="left"
            type="text"
            text={t("pages.chatYo.thinking")}
            title="Assistant"
            titleColor="none"
            date={new Date()}
            forwarded={false}
            replyButton={false}
            removeButton={false}
            status="waiting"
            retracted={false}
            focus={false}
            notch
          />
        )}
      </div>

      <div className={avatarMode ? classes.inputContainer : undefined}>
        <Input
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
                  onClick={handlePlay}
                >
                  🗣
                </button>
                <button
                  className={classes.sendButton}
                  title={t("pages.chatYo.send")}
                  onClick={handleSendMessage}
                >
                  {loading ? "…" : "↑"}
                </button>
              </div>
            ) : (
              <button
                className={classes.sendButton}
                onClick={handleSendMessage}
              >
                {loading ? "…" : "↑"}
              </button>
            )
          }
        />
      </div>
    </>
  );
};
