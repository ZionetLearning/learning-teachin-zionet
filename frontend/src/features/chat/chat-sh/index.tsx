import { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
// import { sendChatMessage } from "./services";
import { useChat } from "@/hooks";

import aiAvatar from "./assets/ai-avatar.svg";
import { useStyles } from "./style";

export const ChatSh = () => {
  const { t } = useTranslation();
  const classes = useStyles();
  const { sendMessage, messages, loading, setMessages } = useChat();

  const [input, setInput] = useState("");
  const [displayedAIMessage, setDisplayedAIMessage] = useState("");

  const chatContainerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (chatContainerRef.current) {
      chatContainerRef.current.scrollTop =
        chatContainerRef.current.scrollHeight;
    }
  }, [messages, displayedAIMessage]);

  const handleSend = () => {
    if (!input.trim()) return;

    const animateAssistantMessage = (text: string) => {
      setDisplayedAIMessage("");
      let index = 0;

      const interval = setInterval(() => {
        setDisplayedAIMessage((prev) => prev + text[index]);
        index++;

        if (index >= text.length) {
          clearInterval(interval);
          setDisplayedAIMessage("");

          // ✅ add assistant message to full messages list
          setMessages((prev) => [
            ...prev,
            { role: "assistant", text }, // internal format
          ]);
        }
      }, 25);
    };

    sendMessage(input, animateAssistantMessage); // hook handles user message
    setInput("");
  };

  return (
    <div className={classes.chatWrapper}>
      <div className={classes.chatTitle}>
        {t("pages.chatSh.azureOpenAiChat")}
      </div>

      <div ref={chatContainerRef} className={classes.chatContainer}>
        {messages.map((msg, idx) => (
          <div
            key={idx}
            className={classes.msgWrapper}
            style={{
              flexDirection: msg.role === "user" ? "row-reverse" : "row",
            }}
          >
            {msg.role === "assistant" && (
              <img src={aiAvatar} alt="avatar" className={classes.avatarImg} />
            )}
            <div
              className={classes.msgBubble}
              style={{
                background: msg.role === "user" ? "#a6d6ff" : "#e2e2e2",
              }}
            >
              {msg.content}
            </div>
          </div>
        ))}

        {displayedAIMessage && (
          <div className={classes.msgWrapper}>
            <img src={aiAvatar} alt="avatar" className={classes.avatarImg} />
            <div className={classes.msgBubbleAI}>{displayedAIMessage}</div>
          </div>
        )}
      </div>

      <div className={classes.inputAndBtnWrapper}>
        <input
          className={classes.input}
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder={t("pages.chatSh.typeYourMessage")}
          disabled={loading}
          onKeyDown={(e) => e.key === "Enter" && handleSend()}
        />
        <button
          onClick={handleSend}
          disabled={loading}
          className={classes.sendBtn}
          style={{
            background: loading ? "#aaa" : "#4A90E2",
            cursor: loading ? "not-allowed" : "pointer",
          }}
        >
          {loading ? "..." : "➤"}
        </button>
      </div>
    </div>
  );
};
