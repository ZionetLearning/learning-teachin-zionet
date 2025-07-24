import React, { useState } from "react";
import { MessageBox, Input } from "react-chat-elements";
import { useChat } from "./hooks";
import { useStyles } from "./style";
import "react-chat-elements/dist/main.css";


export const ChatYo = () => {
  const classes = useStyles();
  const [input, setInput] = useState("");
  const { sendMessage, loading, messages } = useChat();
  const handleSend = () => {
    sendMessage(input);
    setInput("");
  };


  return (
    <div className={classes.chatWrapper}>
      <div className={classes.messagesList}>
        {messages.map((msg, i) => (

          //@ts-ignore  
          <MessageBox
            className={classes.messageBox}
            styles={{
              backgroundColor: msg.position === "right" ? "#11bbff" : "#FFFFFF",
              color: "#000",
            }}
            key={i}
            position={msg.position}
            type="text"
            text={msg.text}
            date={msg.date}
          />

        ))}
        {loading && (
          //@ts-ignore
          <MessageBox
            position="left"
            type="text"
            text="Thinking..."
            date={new Date()}
          />
        )}
      </div>

      <Input
        placeholder="Type a message..."
        className={classes.input}
        value={input}
        onChange={(e: React.ChangeEvent<HTMLInputElement>) => setInput(e.target.value)}
        maxHeight={100}
        onKeyDown={(e) => e.key === "Enter" && handleSend()}
        rightButtons={
          <button className={classes.sendButton} onClick={handleSend}>
            {loading ? "..." : "↑"}
          </button>
        }
      />
    </div>
  );
}

