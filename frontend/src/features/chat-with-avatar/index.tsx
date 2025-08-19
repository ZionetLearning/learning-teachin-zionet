import { useEffect, useState, useRef } from "react";
import { useTranslation } from "react-i18next";
import { useChat } from "@/hooks/useChat";
import { useAvatarSpeech } from "@/hooks";
import { ReactChatElements } from "@/components";
import avatar from "@/assets/avatar.svg";
import { lipsArray } from "@/assets/lips";
import { useStyles } from "./style";

export const ChatWithAvatar = () => {
  const classes = useStyles();
  const { t } = useTranslation();
  const { sendMessage, loading, messages } = useChat();
  const [text, setText] = useState("");
  const { currentVisemeSrc, speak } = useAvatarSpeech({ lipsArray });
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
      <ReactChatElements
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
