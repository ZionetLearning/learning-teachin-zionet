import { useEffect, useState, useRef } from "react";
import { useTranslation } from "react-i18next";
import { useAvatarSpeech, useChat } from "@student/hooks";
import { ReactChatElements } from "@student/components";
import { ChatHistory } from "./components";
import avatar from "@student/assets/avatar.svg";
import { lipsArray } from "@student/assets/lips";
import { useStyles } from "./style";

export const ChatWithAvatar = () => {
  const classes = useStyles();
  const { t, i18n } = useTranslation();
  const { 
    sendMessage, 
    loading, 
    messages,
    allChats,
    isLoadingChats,
    chatHistory,
    isLoadingHistory,
    loadChatHistory,
    loadHistoryIntoMessages,
    startNewChat,
    threadId
  } = useChat();
  const [text, setText] = useState("");
  const [showSidebar, setShowSidebar] = useState(false);
  const [lastHistoryLoadTime, setLastHistoryLoadTime] = useState<number>(0);
  const { currentVisemeSrc, speak } = useAvatarSpeech({ lipsArray });
  const lastSpokenTextRef = useRef<string | null>(null);
  const isRTL = i18n.language === 'he';

  useEffect(() => {
    const now = Date.now();
    const isRecentHistoryLoad = now - lastHistoryLoadTime < 1000;

    if (isRecentHistoryLoad) return;
    
    const last = messages[messages.length - 1];
    if (
      last?.position === "left" &&
      last.text &&
      last.text !== lastSpokenTextRef.current &&
      messages.length > 0
    ) {
      speak(last.text);
      lastSpokenTextRef.current = last.text;
    }
  }, [messages, speak, lastHistoryLoadTime]);
  
  useEffect(() => {
    if (chatHistory && chatHistory.messages.length > 0) {
      setLastHistoryLoadTime(Date.now());
      loadHistoryIntoMessages();
    }
  }, [chatHistory, loadHistoryIntoMessages]);

  const handleSend = () => {
    if (!text.trim()) return;
    sendMessage(text);
    setText("");
  };

  const handleChatSelect = (chatId: string) => {
    setLastHistoryLoadTime(Date.now());
    lastSpokenTextRef.current = null;
    loadChatHistory(chatId);
    setShowSidebar(false);
  };

  const handleNewChat = () => {
    lastSpokenTextRef.current = null;
    startNewChat();
    setShowSidebar(false);
  };

  const handleToggleSidebar = () => {
    setShowSidebar(!showSidebar);
  };

  const handleCloseSidebar = () => {
    setShowSidebar(false);
  };

  return (
    <div className={classes.chatWrapper} dir={isRTL ? 'rtl' : 'ltr'}>
      <ChatHistory
        allChats={allChats}
        isLoadingChats={isLoadingChats}
        isLoadingHistory={isLoadingHistory}
        currentThreadId={threadId}
        showSidebar={showSidebar}
        onChatSelect={handleChatSelect}
        onNewChat={handleNewChat}
        onToggleSidebar={handleToggleSidebar}
        onCloseSidebar={handleCloseSidebar}
      />

      {/* Main Chat Area */}
      <div className={`${classes.mainContent} ${showSidebar ? classes.mainContentShifted : ''}`}>
        {/* Avatar Section */}
        <div className={classes.wrapper}>
          <img
            src={avatar}
            alt={t("pages.chatAvatar.avatar")}
            className={classes.avatar}
          />
          <img
            src={currentVisemeSrc}
            alt={t("pages.chatAvatar.lips")}
            className={classes.lipsImage}
          />
        </div>
        
        {/* Chat Messages and Input */}
        <div className={classes.chatElementsWrapper}>
          <ReactChatElements
            loading={loading}
            messages={messages}
            avatarMode
            value={text}
            onChange={setText}
            handleSendMessage={handleSend}
            handlePlay={() => speak(lastSpokenTextRef.current ?? "")}
          />
        </div>
      </div>
    </div>
  );
};
