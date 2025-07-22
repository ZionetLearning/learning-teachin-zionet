import { useEffect, useRef, useState } from "react";
import { sendChatMessage } from "./services";

export const ChatSh = () => {
  const [messages, setMessages] = useState<{ role: string; content: string }[]>(
    []
  );
  const [input, setInput] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [displayedAIMessage, setDisplayedAIMessage] = useState("");

  const chatContainerRef = useRef<HTMLDivElement>(null);

  // Auto-scroll to bottom
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
        margin: "40px auto",
        display: "flex",
        flexDirection: "column",
        fontFamily: "Arial, sans-serif",
        border: "1px solid #ddd",
        borderRadius: "12px",
        boxShadow: "0 4px 12px rgba(0,0,0,0.1)",
        overflow: "hidden",
        height: "70vh",
      }}
    >
      <div
        style={{
          background: "#4A90E2",
          color: "white",
          padding: "20px",
          fontSize: "24px",
          fontWeight: "bold",
          textAlign: "center",
        }}
      >
        AI Chat
      </div>

      <div
        ref={chatContainerRef}
        style={{
          flex: 1,
          padding: "20px",
          display: "flex",
          flexDirection: "column",
          gap: "12px",
          overflowY: "auto",
          height: "500px",
        }}
      >
        {messages.map((msg, idx) => (
          <div
            key={idx}
            style={{
              alignSelf: msg.role === "user" ? "flex-end" : "flex-start",
              background: msg.role === "user" ? "#4CAF50" : "#f0f0f0",
              color: msg.role === "user" ? "white" : "#333",
              padding: "12px 16px",
              borderRadius: "20px",
              maxWidth: "70%",
              wordWrap: "break-word",
              fontSize: "16px",
              transition: "all 0.3s ease",
            }}
          >
            {msg.content}
          </div>
        ))}

        {displayedAIMessage && (
          <div
            style={{
              alignSelf: "flex-start",
              background: "#f0f0f0",
              padding: "12px 16px",
              borderRadius: "20px",
              maxWidth: "70%",
              fontSize: "16px",
              wordWrap: "break-word",
              fontStyle: "italic",
            }}
          >
            {displayedAIMessage}
          </div>
        )}
      </div>

      <div
        style={{
          display: "flex",
          padding: "15px",
          borderTop: "1px solid #ddd",
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
          }}
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="Type your message..."
          disabled={isLoading}
          onKeyDown={(e) => {
            if (e.key === "Enter") handleSend();
          }}
        />
        <button
          style={{
            background: isLoading ? "#aaa" : "#4A90E2",
            color: "white",
            fontSize: "16px",
            border: "none",
            borderRadius: "8px",
            padding: "12px 20px",
            cursor: isLoading ? "not-allowed" : "pointer",
          }}
          onClick={handleSend}
          disabled={isLoading}
        >
          {isLoading ? "..." : "Send"}
        </button>
      </div>
    </div>
  );
};
