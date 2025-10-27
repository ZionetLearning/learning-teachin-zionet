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
  const lastSpokenTextRef = useRef<string | null>(null);
  const lastUnmuteTimeRef = useRef<number>(0);
  const isLoadingHistoryRef = useRef<boolean>(false);
  const suppressSpeechUntilUserMessageRef = useRef<boolean>(false);

  const [visibleTool, setVisibleTool] = useState<string>(""); // state to show current tool call
  const lingerMs = 1200; // delayed in 1200 ms so we can see the tool call after tool ends
  const hideTimerRef = useRef<number | null>(null);

  const isRTL = i18n.language === "he";

  useEffect(
    function handleToolCallStart() {
      if (currentStage === "Tool" && currentToolCall) {
        if (hideTimerRef.current) {
          window.clearTimeout(hideTimerRef.current);
          hideTimerRef.current = null;
        }
        setVisibleTool(currentToolCall);
      }
    },
    [currentStage, currentToolCall],
  );

  useEffect(
    function handleToolCallEnd() {
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
    },
    [currentStage, visibleTool],
  );

  useEffect(
    function handleAvatarSpeech() {
      const now = Date.now();
      const isRecentUnmute = now - lastUnmuteTimeRef.current < 500;

      if (suppressSpeechUntilUserMessageRef.current) return;
      if (isRecentUnmute) return;
      if (isLoadingHistory) return;
      if (isLoadingHistoryRef.current) return;
      if (isMuted) return;

      const last = messages[messages.length - 1];

      if (
        last?.position === "left" &&
        last.text &&
        last.text !== lastSpokenTextRef.current &&
        !isLoadingHistoryRef.current
      ) {
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
    },
    [messages, speak, stop, isPlaying, threadId, isMuted, isLoadingHistory],
  );

  useEffect(
    function handleChatHistoryLoad() {
      if (chatHistory && chatHistory.messages.length > 0) {
        lastSpokenTextRef.current = null;
        loadHistoryIntoMessages();
        suppressSpeechUntilUserMessageRef.current = true;

        setTimeout(() => {
          isLoadingHistoryRef.current = false;
        }, 500);
      } else if (chatHistory && chatHistory.messages.length === 0) {
        isLoadingHistoryRef.current = false;
        suppressSpeechUntilUserMessageRef.current = true;
      }
    },
    [chatHistory, loadHistoryIntoMessages],
  );

  const handleSend = async () => {
    if (!text.trim()) return;

    // Stop current speech when user sends a new message
    if (isPlaying) {
      await stop();
    }

    suppressSpeechUntilUserMessageRef.current = false;

    sendMessage(text);
    setText("");
  };

  const handleMuteToggle = async () => {
    // If currently playing and about to mute, stop the speech
    if (!isMuted && isPlaying) {
      await stop();
    }

    if (isMuted) {
      lastUnmuteTimeRef.current = Date.now();
    }

    toggleMute();
  };

  const handleChatSelect = async (chatId: string) => {
    // Stop current speech when switching chats
    if (isPlaying) {
      await stop();
    }

    isLoadingHistoryRef.current = true;
    suppressSpeechUntilUserMessageRef.current = true;

    // Clear spoken text reference for new chat
    lastSpokenTextRef.current = null;

    loadChatHistory(chatId);
    setShowSidebar(false);
  };

  const handleNewChat = async () => {
    // Stop current speech when starting new chat
    if (isPlaying) {
      await stop();
    }

    isLoadingHistoryRef.current = true;
    suppressSpeechUntilUserMessageRef.current = true;

    // Clear references for new chat
    lastSpokenTextRef.current = null;

    startNewChat();
    setShowSidebar(false);

    setTimeout(() => {
      isLoadingHistoryRef.current = false;
    }, 500);
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
            messages={messages}
            value={text}
            onChange={setText}
            handleSendMessage={handleSend}
          />
        </div>
      </div>
    </div>
  );
};
