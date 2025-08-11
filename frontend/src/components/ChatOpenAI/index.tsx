// ChatOpenAI.tsx
import { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useChat } from "@/hooks";
import aiAvatar from "@/assets/avatar.svg";

type ChatOpenAIProps = {
  onAssistantText?: (text: string) => void | Promise<void>;
  headerAvatar?: React.ReactNode; // 👈 LIVE avatar goes here (top header)
  title?: string;
};

const Spinner = () => (
  <svg viewBox="0 0 50 50" width="18" height="18">
    <circle
      cx="25"
      cy="25"
      r="20"
      fill="none"
      stroke="currentColor"
      strokeWidth="4"
      strokeLinecap="round"
      strokeDasharray="31.4 31.4"
    >
      <animateTransform
        attributeName="transform"
        type="rotate"
        from="0 25 25"
        to="360 25 25"
        dur="0.9s"
        repeatCount="indefinite"
      />
    </circle>
  </svg>
);

export const ChatOpenAI = ({
  onAssistantText,
  headerAvatar,
  title,
}: ChatOpenAIProps) => {
  const { t } = useTranslation();
  const { sendMessage, messages, loading, setMessages } = useChat();

  const [input, setInput] = useState("");
  const [awaitingAssistant, setAwaitingAssistant] = useState(false);
  const chatRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (chatRef.current)
      chatRef.current.scrollTop = chatRef.current.scrollHeight;
  }, [messages, awaitingAssistant]);

  const handleSend = () => {
    if (!input.trim()) return;

    setAwaitingAssistant(true);

    const onAssistantReady = async (text: string) => {
      setAwaitingAssistant(false);
      await onAssistantText?.(text);
      setMessages((prev) => [...prev, { role: "assistant", text }]);
    };

    sendMessage(input, onAssistantReady);
    setInput("");
  };

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        gap: 12,
        padding: "2%",
        width: "50%",
        alignContent: "center",
        margin: "0 auto",
        background: "#fff",
        borderRadius: 8,
        boxShadow: "0 2px 4px rgba(0,0,0,0.1)",
      }}
    >
      {/* Header with LIVE avatar */}
      <div
        style={{
          position: "sticky",
          top: 0,
          zIndex: 1,
          display: "flex",
          alignItems: "center",
          gap: 12,
          padding: "8px 10px",
          borderBottom: "1px solid #eee",
          background: "#fff",
          borderRadius: 8,
        }}
      >
        {headerAvatar && (
          <div style={{ width: 56, height: 56 }}>{headerAvatar}</div>
        )}
        <div style={{ fontWeight: 600, fontSize: 16 }}>
          {title ?? t("pages.chatSh.azureOpenAiChat")}
        </div>
      </div>

      <div
        ref={chatRef}
        style={{
          height: "60vh",
          overflowY: "auto",
          border: "1px solid #ddd",
          borderRadius: 8,
          padding: 12,
          display: "grid",
          gap: 8,
        }}
      >
        {messages.map((msg, idx) => (
          <div
            key={idx}
            style={{
              display: "flex",
              flexDirection: msg.role === "user" ? "row-reverse" : "row",
              gap: 8,
            }}
          >
            {/* Small static avatar next to assistant messages (kept) */}
            {msg.role === "assistant" ? (
              <img
                src={aiAvatar}
                alt="avatar"
                style={{ width: 36, height: 36 }}
              />
            ) : null}

            <div
              style={{
                background: msg.role === "user" ? "#a6d6ff" : "#e2e2e2",
                borderRadius: 12,
                padding: "8px 10px",
                maxWidth: "75%",
                height: "fit-content",
              }}
            >
              {msg.content}
            </div>
          </div>
        ))}

        {/* Simple loader bubble (no dots) */}
        {awaitingAssistant && (
          <div style={{ display: "flex", gap: 8 }}>
            <img
              src={aiAvatar}
              alt="avatar"
              style={{ width: 36, height: 36 }}
            />
            <div
              style={{
                background: "#e2e2e2",
                borderRadius: 12,
                padding: "8px 10px",
                maxWidth: "75%",
                display: "inline-flex",
                alignItems: "center",
                gap: 6,
                height: "fit-content",
              }}
            >
              <Spinner />
              <span>{t("common.thinking", "Thinking")}</span>
            </div>
          </div>
        )}
      </div>

      <div style={{ display: "flex", gap: 8 }}>
        <input
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder={t("pages.chatSh.typeYourMessage")}
          disabled={loading || awaitingAssistant}
          onKeyDown={(e) => e.key === "Enter" && handleSend()}
          style={{
            flex: 1,
            padding: "8px 10px",
            borderRadius: 8,
            border: "1px solid #ccc",
          }}
        />
        <button
          onClick={handleSend}
          disabled={loading || awaitingAssistant}
          style={{
            padding: "8px 14px",
            borderRadius: 8,
            border: 0,
            background: loading || awaitingAssistant ? "#aaa" : "#4A90E2",
            color: "white",
            cursor: loading || awaitingAssistant ? "not-allowed" : "pointer",
          }}
        >
          {loading || awaitingAssistant ? "..." : "➤"}
        </button>
      </div>
    </div>
  );
};
