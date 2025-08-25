import { useEffect, useRef, useState } from "react";

import { ChatHeader, ChatInput, ChatMessage } from "../components";
import { useChatContext } from "../context/chat-context";
import { useChat } from "../hooks";

import useStyles from "../style";

export const SidebarChat = () => {
  const { sendMessage } = useChat();
  const { state } = useChatContext();
  const classes = useStyles();

  const bottomRef = useRef<HTMLDivElement>(null);

  const [input, setInput] = useState("");

  useEffect(
    function scrollDownOnNewMessage() {
      if (state.messages.length > 0) {
        bottomRef.current?.scrollIntoView({ behavior: "smooth" });
      }
    },
    [state.messages],
  );

  const botTyping = state.messages.some(
    (m) => m.sender === "bot" && !m.isComplete,
  );

  return (
    <aside className={classes.sidebar}>
      <ChatHeader />
      <main className={classes.messagesContainer}>
        {state.messages.map((message) => (
          <ChatMessage key={message.id} message={message} />
        ))}
        <div ref={bottomRef} />
      </main>
      <ChatInput
        input={input}
        setInput={setInput}
        sendMessage={sendMessage}
        disabled={botTyping}
      />
    </aside>
  );
};
