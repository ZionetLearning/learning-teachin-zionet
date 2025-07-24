import { useEffect, useRef, useState } from "react";
import { sendChatMessage } from "./services";
import aiAvatar from "./assets/ai-avatar.svg";
import { useStyles } from "./style";

export const ChatSh = () => {
  const classes = useStyles();
  const [messages, setMessages] = useState<{ role: string; content: string }[]>(
    [],
  );
  const [input, setInput] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [displayedAIMessage, setDisplayedAIMessage] = useState("");

  const chatContainerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (chatContainerRef.current) {
      chatContainerRef.current.scrollTop =
        chatContainerRef.current.scrollHeight;
    }
  }, [messages, displayedAIMessage]);

  const handleSend = async () => {
    if (!input.trim()) return;
    const userMessage = { role: "user", content: input };
    setMessages((prev) => [...prev, userMessage]);
    setInput("");
    setIsLoading(true);

    try {
      const response = await sendChatMessage(input);
      setDisplayedAIMessage("");
      let index = 0;
      const interval = setInterval(() => {
        setDisplayedAIMessage((prev) => prev + response[index]);
        index++;
        if (index >= response.length) {
          clearInterval(interval);
          setMessages((prev) => [
            ...prev,
            { role: "assistant", content: response },
          ]);
          setDisplayedAIMessage("");
          setIsLoading(false);
        }
      }, 25);
    } catch (error) {
      alert("Error: " + error);
      setIsLoading(false);
    }
  };

  return (
    <div className={classes.chatWrapper}>
      <div className={classes.chatTitle}>Azure OpenAI Chat</div>
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
          placeholder="Type your message..."
          disabled={isLoading}
          onKeyDown={(e) => e.key === "Enter" && handleSend()}
        />
        <button
          onClick={handleSend}
          disabled={isLoading}
          className={classes.sendBtn}
          style={{
            background: isLoading ? "#aaa" : "#4A90E2",
            cursor: isLoading ? "not-allowed" : "pointer",
          }}
        >
          {isLoading ? "..." : "➤"}
        </button>
      </div>
    </div>
  );
};
