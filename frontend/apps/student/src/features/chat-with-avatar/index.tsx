import { useEffect, useState, useRef } from "react";
import { useTranslation } from "react-i18next";
import { IconButton } from "@mui/material";
import VolumeUpIcon from "@mui/icons-material/VolumeUp";
import VolumeOffIcon from "@mui/icons-material/VolumeOff";
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
    threadId,
    currentStage,
    currentToolCall,
  } = useChat();

  const { currentVisemeSrc, speak, stop, isPlaying, toggleMute, isMuted } =
    useAvatarSpeech({ lipsArray });

  const [text, setText] = useState("");
  const [showSidebar, setShowSidebar] = useState(false);
  const [lastHistoryLoadTime, setLastHistoryLoadTime] = useState<number>(0);
  const lastSpokenTextRef = useRef<string | null>(null);
  const currentThreadIdRef = useRef<string | undefined>(null); // Track current thread

  const [visibleTool, setVisibleTool] = useState<string>(""); // state to show current tool call
  const lingerMs = 1200; // delayed in 1200 ms so we can see the tool call after tool ends
  const hideTimerRef = useRef<number | null>(null);

  const isRTL = i18n.language === "he";

  // When a tool starts, show immediately and cancel any hide timer
  useEffect(() => {
    if (currentStage === "Tool" && currentToolCall) {
      if (hideTimerRef.current) {
        window.clearTimeout(hideTimerRef.current);
        hideTimerRef.current = null;
      }
      setVisibleTool(currentToolCall);
    }
  }, [currentStage, currentToolCall]);

  // When we leave Tool, keep it for a short time then hide
  useEffect(() => {
    if (currentStage !== "Tool" && visibleTool) {
      if (hideTimerRef.current) {
        window.clearTimeout(hideTimerRef.current);
      }
      hideTimerRef.current = window.setTimeout(() => {
        setVisibleTool("");
        hideTimerRef.current = null;
      }, lingerMs);
    }
    return () => {
      if (hideTimerRef.current) {
        window.clearTimeout(hideTimerRef.current);
        hideTimerRef.current = null;
      }
    };
  }, [currentStage, visibleTool]);

  // Track thread changes to prevent speaking old messages
  useEffect(() => {
    if (threadId !== currentThreadIdRef.current) {
      currentThreadIdRef.current = threadId;
      lastSpokenTextRef.current = null; // Reset when thread changes
    }
  }, [threadId]);

  // Fixed speech effect with better conditions
  useEffect(() => {
    const now = Date.now();
    const isRecentHistoryLoad = now - lastHistoryLoadTime < 1000;

    if (isRecentHistoryLoad) return;
    if (messages.length === 0) return; // Don't speak if no messages
    if (isMuted) return;
    const last = messages[messages.length - 1];

    if (
      last?.position === "left" &&
      last.text &&
      last.text !== lastSpokenTextRef.current &&
      threadId === currentThreadIdRef.current
    ) {
      // Always stop current speech before speaking new message
      if (isPlaying) {
        stop().then(() => {
          speak(last.text);
          lastSpokenTextRef.current = last.text;
        });
      } else {
        speak(last.text);
        lastSpokenTextRef.current = last.text;
      }
    }
  }, [
    messages,
    speak,
    stop,
    lastHistoryLoadTime,
    isPlaying,
    threadId,
    isMuted,
  ]);

  useEffect(() => {
    if (chatHistory && chatHistory.messages.length > 0) {
      setLastHistoryLoadTime(Date.now());
      loadHistoryIntoMessages();
    }
  }, [chatHistory, loadHistoryIntoMessages]);

  const handleSend = async () => {
    if (!text.trim()) return;

    // Stop current speech when user sends a new message
    if (isPlaying) {
      await stop();
    }

    sendMessage(text);
    setText("");
  };

  const handlePlay = async () => {
    if (isMuted) return;
    const lastMessage = messages[messages.length - 1];
    if (lastMessage?.position === "left" && lastMessage.text) {
      if (isPlaying) {
        await stop();
      } else {
        await speak(lastMessage.text);
        lastSpokenTextRef.current = lastMessage.text;
      }
    }
  };

  const handleStop = async () => {
    await stop();
  };

  const handleMuteToggle = async () => {
    // If currently playing and about to mute, stop the speech
    if (!isMuted && isPlaying) {
      await stop();
    }
    toggleMute();
  };

  const handleChatSelect = async (chatId: string) => {
    setLastHistoryLoadTime(Date.now());

    // Stop current speech when switching chats
    if (isPlaying) {
      await stop();
    }

    // Clear spoken text reference for new chat
    lastSpokenTextRef.current = null;
    currentThreadIdRef.current = null; // Will be updated by useEffect

    loadChatHistory(chatId);
    setShowSidebar(false);
  };

  const handleNewChat = async () => {
    // Stop current speech when starting new chat
    if (isPlaying) {
      await stop();
    }

    // Clear references for new chat
    lastSpokenTextRef.current = null;
    currentThreadIdRef.current = null; // Will be updated by useEffect

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
    <div className={classes.chatWrapper} dir={isRTL ? "rtl" : "ltr"}>
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

      <div
        className={`${classes.mainContent} ${showSidebar ? classes.mainContentShifted : ""}`}
      >
        <IconButton
          onClick={handleMuteToggle}
          className={`${classes.muteButton} ${isMuted ? classes.muteButtonMuted : classes.muteButtonUnmuted}`}
          aria-label={
            isMuted ? t("pages.chatAvatar.unmute") : t("pages.chatAvatar.mute")
          }
          title={
            isMuted ? t("pages.chatAvatar.unmute") : t("pages.chatAvatar.mute")
          }
        >
          {isMuted ? <VolumeOffIcon /> : <VolumeUpIcon />}
        </IconButton>
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

          {/* Tool badge - Renders even after Tool ends—until linger timeout */}
          {!!visibleTool && (
            <div className={classes.toolCallBadge} aria-live="polite">
              {t("pages.chatAvatar.callingTool", { tool: visibleTool })}
            </div>
          )}
        </div>

        <div className={classes.chatElementsWrapper}>
          <ReactChatElements
            loading={loading}
            isPlaying={isPlaying}
            messages={messages}
            avatarMode
            value={text}
            onChange={setText}
            handleSendMessage={handleSend}
            handlePlay={handlePlay}
            handleStop={handleStop}
          />
        </div>
      </div>
    </div>
  );
};
