import { useEffect, useState, useRef } from "react";
import { useChat } from "../../chat/chat-yo/hooks";
import { useAvatarSpeech } from "./hooks";
import avatar from "./assets/avatar.svg";
import { useStyles } from "./style";
import { ChatUi } from "../../chat/chat-yo/components";

type SvgModule = { default: string };

const lips = import.meta.glob("./assets/lips/*.svg", { eager: true });

const lipsArray = Object.values(lips).map((mod) => (mod as SvgModule).default);

export const AvatarSh = () => {
  const classes = useStyles();
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
        <img src={avatar} alt="Avatar" className={classes.avatar} />
        <img src={currentVisemeSrc} alt="Lips" className={classes.lipsImage} />
      </div>
      <ChatUi loading={loading} messages={messages} avatarMode
        value={text}
        onChange={setText}
        handleSendMessage={handleSend} 
        handlePlay={() => speak(lastSpokenTextRef.current ?? "")} />
    </div>
  );
};