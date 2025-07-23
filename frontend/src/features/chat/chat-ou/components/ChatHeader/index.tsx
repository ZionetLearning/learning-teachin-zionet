import React from "react";
import { useStyles } from "./style";

interface ChatHeaderProps {
  title?: string;
  isOnline?: boolean;
  isTyping?: boolean;
  participantCount?: number;
}

const ChatHeader: React.FC<ChatHeaderProps> = ({
  title = "Smart Chat",
  isOnline = true,
  isTyping = false,
  participantCount = 2,
}) => {
  const classes = useStyles();

  const getStatusText = () => {
    if (isTyping) return "AI is typing...";
    if (isOnline) return "Online";
    return "Offline";
  };

  const getStatusIcon = () => {
    if (isTyping) return "⌨️";
    if (isOnline) return "🟢";
    return "🔴";
  };

  return (
    <div className={classes.container}>
      <div className={classes.titleSection}>
        <h2 className={classes.title}>{title}</h2>
        <div className={classes.subtitle}>{participantCount} participants</div>
      </div>

      <div className={classes.statusSection}>
        <div
          className={`${classes.statusIndicator} ${isTyping ? classes.typing : ""}`}
        >
          <span className={classes.statusIcon}>{getStatusIcon()}</span>
          <span className={classes.statusText}>{getStatusText()}</span>
        </div>

        {isTyping && (
          <div className={classes.typingAnimation}>
            <div className={classes.typingDots}>
              <span></span>
              <span></span>
              <span></span>
            </div>
          </div>
        )}
      </div>

      <div className={classes.actions}>
        <button className={classes.actionButton} title="More options">
          ⋯
        </button>
      </div>
    </div>
  );
};

export { ChatHeader };
