import { useEffect, useState, useRef } from "react";
import { useTranslation } from "react-i18next";
import { useChat } from "@/hooks/useChat";
import { useAvatarSpeech } from "@/hooks";
import avatar from "@/assets/avatar.svg";
import { useStyles } from "./style";
import { ChatUi } from "../chat-yo/components";
import { lipsArray } from "@/assets/lips";

export const ChatAvatar = () => {
  const classes = useStyles();
  const { t } = useTranslation();
  const { sendMessage, loading, messages } = useChat();
  const [text, setText] = useState("");
  const { currentVisemeSrc, speak } = useAvatarSpeech(lipsArray);
  const lastSpokenTextRef = useRef<string | null>(null);

  useEffect(() => {
    const last = messages[messages.length - 1];
    if (
      last?.position === "left" &&
      last.text &&
      last.text !== lastSpokenTextRef.current
    ) {
      speak(last.text);
      lastSpokenTextRef.current = last.text;
    }
  }, [messages, speak]);

  const handleSend = () => {
    if (!text.trim()) return;
    sendMessage(text);
    setText("");
  };

  return (
    <div className={classes.chatWrapper}>
      <div className={classes.wrapper}>
        <img
          src={avatar}
          alt={t("pages.chatAvatar.avatar")}
          className={classes.avatar}
        />
        <img
          src={currentVisemeSrc}
          alt={t("pages.chatAvatar.lips")}
          className={classes.lipsImage}
        />
      </div>
      <ChatUi
        loading={loading}
        messages={messages}
        avatarMode
        value={text}
        onChange={setText}
        handleSendMessage={handleSend}
        handlePlay={() => speak(lastSpokenTextRef.current ?? "")}
      />
    </div>
  );
};
