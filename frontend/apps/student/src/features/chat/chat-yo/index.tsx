import { useState } from "react";
import { useChat } from "@/hooks";
import { useStyles } from "./style";
import { ReactChatElements } from "@/components";
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
    <div className={classes.chatWrapper} data-testid="chat-yo-page">
      <ReactChatElements
        messages={messages}
        loading={loading}
        value={input}
        onChange={setInput}
        handleSendMessage={handleSend}
      />
    </div>
  );
};
