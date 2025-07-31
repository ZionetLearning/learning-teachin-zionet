import { useEffect, useState, useRef } from "react";
import { MessageBox, Input } from "react-chat-elements";
import { useChat } from "../../chat/chat-yo/hooks";
import { useAvatarSpeech } from "./hooks/useAvatarSpeech";
import avatar from "./assets/avatar.svg";

import { useStyles } from "./style";

type SvgModule = { default: string };

const lips = import.meta.glob("./assets/lips/*.svg", { eager: true });

const lipsArray = Object.values(lips).map((mod) => (mod as SvgModule).default);

export const AvatarSh = () => {
  const classes = useStyles();
  //const [currentViseme, setCurrentViseme] = useState<number>(0);
  const { sendMessage, loading, messages } = useChat();
  const [text, setText] = useState("");
  const { currentVisemeSrc, speak } = useAvatarSpeech(lipsArray);


  // forcing an error to test Error Boundary of Application Insights Azure
  //  useEffect(() => {
  //   throw new Error("AvatarSh Crashed! This is a test error for Application Insights.");
  // }, []);

  const lastSpokenTextRef = useRef<string | null>(null);

  useEffect(() => {
    const last = messages[messages.length - 1];
    if (last?.position === "left" && last.text && last.text !== lastSpokenTextRef.current) {
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
        <img
          src={currentVisemeSrc}
          alt="Lips"
          className={classes.lipsImage}
        />
      </div>

      <div className={classes.messagesList}>
        {messages.map((msg, i) => (
          <MessageBox
            className={classes.messageBox}
            styles={{
              backgroundColor: msg.position === "right" ? "#11bbff" : "#FFFFFF",
              color: "#000",
            }}
            key={i}
            id={i.toString()}
            position={msg.position}
            type="text"
            text={msg.text}
            title={msg.position === "right" ? "Me" : "Assistant"}
            titleColor={msg.position === "right" ? "black" : "gray"}
            date={msg.date}
            forwarded={false}
            replyButton={true}
            removeButton={true}
            status={"received"}
            notch={true}
            focus={false}
            retracted={false}

          />
        ))}

        {loading && (
          <MessageBox
            id="assistant"
            position="left"
            type="text"
            text="Thinking..."
            title="Assistant"
            titleColor="none"
            date={new Date()}
            forwarded={false}
            replyButton={false}
            removeButton={false}
            status={"waiting"}
            notch={true}
            focus={false}
            retracted={false}
          />
        )}
      </div>

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
            <button className={classes.sendButton} onClick={() => speak(text)}>
              🗣
            </button>
            <button className={classes.sendButton} onClick={handleSend}>
              {loading ? "..." : "↑"}
            </button>
          </div>
        }
      />
    </div>
  );


  /*return (
    <div>
      <div className={classes.wrapper}>
        <img src={avatar} alt="Avatar" className={classes.avatar} />
        <img
          src={currentVisemeSrc}
          alt="Lips"
          className={classes.lipsImage}
        />
      </div>
      <div style={{ marginTop: "20px" }}>
        <input
          type="text"
          placeholder="כתוב פה משהו בעברית"
          value={text}
          onChange={(e) => setText(e.target.value)}
          className={classes.input}
          dir="rtl"
        />
        <br />
        <button onClick={handleSend} className={classes.button}>
          דברי
        </button>
      </div>
    </div>
  );*/
};
