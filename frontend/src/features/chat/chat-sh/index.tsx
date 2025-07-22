import { useEffect, useRef, useState } from "react";
import { sendChatMessage } from "./services";
import aiAvatar from "./assets/ai-avatar.svg";

export const ChatSh = () => {
  const [messages, setMessages] = useState<{ role: string; content: string }[]>(
    []
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
    <div
      style={{
        maxWidth: "700px",
        margin: "30px auto",
        display: "flex",
        flexDirection: "column",
        fontFamily: "Arial, sans-serif",
        border: "1px solid #ddd",
        borderRadius: "12px",
        overflow: "hidden",
        boxShadow: "0 6px 20px rgba(0,0,0,0.1)",
        height: "70vh",
      }}
    >
      <div
        style={{
          background: "#cce6ff",
          padding: "18px",
          fontSize: "24px",
          fontWeight: "bold",
          textAlign: "center",
          color: "#3c3c3c",
        }}
      >
        Azure OpenAI Chat
      </div>

      <div
        ref={chatContainerRef}
        style={{
          flex: 1,
          padding: "20px",
          display: "flex",
          flexDirection: "column",
          gap: "15px",
          overflowY: "auto",
          height: "60vh",
          background: "#f9f9f9",
        }}
      >
        {messages.map((msg, idx) => (
          <div
            key={idx}
            style={{
              display: "flex",
              flexDirection: msg.role === "user" ? "row-reverse" : "row",
              alignItems: "flex-end",
              gap: "10px",
            }}
          >
            {msg.role === "assistant" && (
              <img
                src={aiAvatar}
                alt="avatar"
                style={{ width: "40px", height: "40px", borderRadius: "50%" }}
              />
            )}
            <div
              style={{
                background: msg.role === "user" ? "#a6d6ff" : "#e2e2e2",
                color: "#333",
                padding: "12px 16px",
                borderRadius: "16px",
                maxWidth: "65%",
                wordWrap: "break-word",
                fontSize: "16px",
                lineHeight: "1.5",
                textAlign: "left",
              }}
            >
              {msg.content}
            </div>
          </div>
        ))}

        {displayedAIMessage && (
          <div
            style={{
              display: "flex",
              flexDirection: "row",
              alignItems: "flex-end",
              gap: "10px",
            }}
          >
            <img
              src={aiAvatar}
              alt="avatar"
              style={{ width: "40px", height: "40px", borderRadius: "50%" }}
            />
            <div
              style={{
                background: "#e2e2e2",
                color: "#333",
                padding: "12px 16px",
                borderRadius: "16px",
                maxWidth: "65%",
                fontSize: "16px",
                                textAlign: "left",

              }}
            >
              {displayedAIMessage}
            </div>
          </div>
        )}
      </div>

      <div
        style={{
          display: "flex",
          borderTop: "1px solid #ddd",
          background: "#f0f0f0",
          padding: "15px",
          gap: "10px",
        }}
      >
        <input
          style={{
            flex: 1,
            padding: "12px",
            fontSize: "16px",
            borderRadius: "8px",
            border: "1px solid #ccc",
            background: "#ffffff",
          }}
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="Type your message..."
          disabled={isLoading}
          onKeyDown={(e) => e.key === "Enter" && handleSend()}
        />
        <button
          onClick={handleSend}
          disabled={isLoading}
          style={{
            background: isLoading ? "#aaa" : "#4A90E2",
            color: "white",
            fontSize: "16px",
            borderRadius: "8px",
            border: "none",
            padding: "0 20px",
            cursor: isLoading ? "not-allowed" : "pointer",
          }}
        >
          {isLoading ? "..." : "➤"}
        </button>
      </div>
    </div>
  );
};
