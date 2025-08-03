import { useEffect, useState, useRef } from "react";
import { Input } from "react-chat-elements";
import { useChat } from "../../chat/chat-yo/hooks";
import { useAvatarSpeech } from "./hooks";
import avatar from "./assets/avatar.svg";
import { MessageBox } from "./components";
import { useStyles } from "./style";

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

      <div className={classes.messagesList}>
        {messages.map((msg, i) => (
          <MessageBox className={classes.messageBox} key={i} message={msg} />
        ))}

        {loading && <MessageBox message={undefined} loading />}
      </div>
      <div className={classes.inputContainer}>
        <Input
          placeholder="כתוב הודעה..."
          className={classes.input}
          value={text}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
            setText(e.target.value)
          }
          maxHeight={100}
          onKeyDown={(e) => e.key === "Enter" && handleSend()}
          rightButtons={
            <div className={classes.rightButtons}>
              <button
                className={classes.sendButton}
                onClick={() => speak(lastSpokenTextRef.current ?? "")}
              >
                🗣
              </button>
              <button className={classes.sendButton} onClick={handleSend}>
                {loading ? "..." : "↑"}
              </button>
            </div>
          }
        />
      </div>
    </div>
  );
};
