import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  container: {
    display: "flex",
    flexDirection: "column",
    height: "100%",
    overflow: "hidden",
    position: "relative",
  },

  messagesList: {
    flex: 1,
    overflowY: "auto",
    padding: "16px",
    display: "flex",
    flexDirection: "column",
    gap: "8px",

    "&::-webkit-scrollbar": {
      width: "6px",
    },
    "&::-webkit-scrollbar-track": {
      background: "#f1f1f1",
      borderRadius: "3px",
    },
    "&::-webkit-scrollbar-thumb": {
      background: "#c1c1c1",
      borderRadius: "3px",
      "&:hover": {
        background: "#a8a8a8",
      },
    },
  },

  messageGroup: {
    display: "flex",
    flexDirection: "column",
    gap: "4px",
  },

  dateHeader: {
    textAlign: "center",
    padding: "8px 16px",
    margin: "16px 0 8px 0",
    fontSize: "12px",
    fontWeight: 500,
    color: "#666",
    backgroundColor: "#f5f5f5",
    borderRadius: "16px",
    alignSelf: "center",
    maxWidth: "fit-content",
  },

  messageWrapper: {
    display: "flex",
    flexDirection: "column",
    marginBottom: "16px",
  },

  consecutiveMessage: {
    marginBottom: "4px",

    "& > div": {
      "& > div:first-child": {
        display: "none",
      },
    },
  },

  emptyState: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "center",
    height: "100%",
    textAlign: "center",
    color: "#666",
    padding: "32px",
  },

  emptyStateIcon: {
    fontSize: "48px",
    marginBottom: "16px",
    opacity: 0.5,
  },

  emptyStateText: {
    fontSize: "16px",
    lineHeight: 1.5,
    maxWidth: "300px",
  },

  loadingIndicator: {
    display: "flex",
    alignItems: "center",
    gap: "12px",
    padding: "16px",
    marginTop: "8px",
    alignSelf: "flex-start",
  },

  typingDots: {
    display: "flex",
    gap: "4px",

    "& span": {
      width: "8px",
      height: "8px",
      borderRadius: "50%",
      backgroundColor: "#007bff",
      animation: "$typing 1.4s infinite ease-in-out",

      "&:nth-child(1)": {
        animationDelay: "0s",
      },
      "&:nth-child(2)": {
        animationDelay: "0.2s",
      },
      "&:nth-child(3)": {
        animationDelay: "0.4s",
      },
    },
  },

  typingText: {
    fontSize: "14px",
    color: "#666",
    fontStyle: "italic",
  },

  scrollToBottomButton: {
    position: "absolute",
    bottom: "20px",
    right: "20px",
    width: "40px",
    height: "40px",
    borderRadius: "50%",
    backgroundColor: "#007bff",
    color: "white",
    border: "none",
    cursor: "pointer",
    fontSize: "18px",
    fontWeight: "bold",
    boxShadow: "0 2px 8px rgba(0, 123, 255, 0.3)",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    transition: "all 0.2s ease",
    zIndex: 10,

    "&:hover": {
      backgroundColor: "#0056b3",
      transform: "scale(1.1)",
      boxShadow: "0 4px 12px rgba(0, 123, 255, 0.4)",
    },

    "&:active": {
      transform: "scale(0.95)",
    },
  },

  "@keyframes typing": {
    "0%, 60%, 100%": {
      transform: "translateY(0)",
      opacity: 0.5,
    },
    "30%": {
      transform: "translateY(-10px)",
      opacity: 1,
    },
  },

  "@media (max-width: 768px)": {
    messagesList: {
      padding: "12px",
    },

    dateHeader: {
      fontSize: "11px",
      padding: "6px 12px",
    },

    messageWrapper: {
      marginBottom: "12px",
    },

    consecutiveMessage: {
      marginBottom: "3px",
    },
  },
});
