import { useEffect, useState, useRef } from "react";
import { useTranslation } from "react-i18next";
import { useChat } from "../chat-yo/hooks";
import { useAvatarSpeech } from "../../avatar/avatar-sh/hooks";
import avatar from "../../avatar/avatar-sh/assets/avatar.svg";
import { useStyles } from "./style";
import { ChatUi } from "../chat-yo/components";

type SvgModule = { default: string };

const lips = import.meta.glob("../../avatar/avatar-sh/assets/lips/*.svg", {
  eager: true,
});

const lipsArray = Object.values(lips).map((mod) => (mod as SvgModule).default);

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
