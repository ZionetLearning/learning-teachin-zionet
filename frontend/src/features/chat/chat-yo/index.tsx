import React, { useState } from "react";
import { MessageBox, Input } from "react-chat-elements";
import { useChat } from "./hooks";
import { useStyles } from "./style";
import avatar from "./assets/avatar1.png";
import "react-chat-elements/dist/main.css";


export const ChatYo = () => {
  const classes = useStyles();
  const [input, setInput] = useState("");
  const { sendMessage, loading, messages } = useChat();
  const avatarUrl = avatar;
  const handleSend = () => {
    sendMessage(input);
    setInput("");
  };


  return (
    <div className={classes.chatWrapper}>
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
            avatar={msg.position === "left" ? avatarUrl : undefined}
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

