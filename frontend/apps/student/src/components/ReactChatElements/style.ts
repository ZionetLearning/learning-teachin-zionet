import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  chatContainer: {
    display: "flex",
    flexDirection: "column",
    height: "100%",
  },
  messagesList: {
    height: "500px",
    overflowX: "auto",
    overflowY: "auto",
    marginBottom: 10,
    "[dir='rtl'] &": {
      direction: "rtl",
    },
  },
  messagesListAvatar: {
    flex: 1,
    overflowY: "auto",
    overflowX: "hidden",
    marginBottom: 10,
    "[dir='rtl'] &": {
      direction: "rtl",
    },
  },
  messageBox: {
    "& .rce-mbox-right-notch": {
      fill: "#11bbff !important",
    },
    "& .rce-container-mbox-right": {
      flexDirection: "row-reverse",
      "[dir='rtl'] &": {
        flexDirection: "row",
      },
    },
    "& .rce-container-mbox-left": {
      "[dir='rtl'] &": {
        flexDirection: "row-reverse",
      },
    },
    "& .rce-mbox-right .rce-mbox-title": {
      textAlign: "right",
      justifyContent: "flex-end",
      "[dir='rtl'] &": {
        textAlign: "left",
        justifyContent: "flex-start",
      },
    },
    "& .rce-mbox-left .rce-mbox-title": {
      "[dir='rtl'] &": {
        textAlign: "right",
        justifyContent: "flex-end",
      },
    },
    "& .rce-mbox-body": {
      "[dir='rtl'] &": {
        textAlign: "right",
      },
    },
    "& .rce-mbox-text": {
      "[dir='rtl'] &": {
        textAlign: "right",
      },
    },
    "& svg": {
      display: "none !important",
    },
  },
  inputContainer: {
    flexShrink: 0,
    padding: 6,
    "[dir='rtl'] &": {
      direction: "rtl",
    },
  },
  input: {
    border: "1px solid #ddd",
    borderRadius: "0%",
    paddingLeft: "6px",
    "[dir='rtl'] &": {
      paddingLeft: 0,
      paddingRight: "6px",
      textAlign: "right",
    },
    "& input": {
      color: "black",
      "[dir='rtl'] &": {
        textAlign: "right",
      },
      "@media (prefers-color-scheme: dark)": {
        color: "white",
      },
    },
  },
  inputAvatar: {
    width: "100% !important",
    fontSize: "18px",
    padding: "1px 6px 1px 6px",
    border: "1px solid #ccc",
    borderRadius: 5,
    boxSizing: "border-box",
    "[dir='rtl'] &": {
      textAlign: "right",
    },
    "& input": {
      color: "black",
      "[dir='rtl'] &": {
        textAlign: "right",
      },
      "@media (prefers-color-scheme: dark)": {
        color: "white",
      },
    },
  },
  rightButtons: {
    display: "flex",
    flexDirection: "row",
    gap: "5px",
  },
  sendButton: {
    backgroundColor: "#44bbff",
    color: "#fff",
    borderRadius: "50%",
    width: "35px",
    height: "30px",
    fontSize: "22px",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    padding: 0,
    lineHeight: 1,
  },
  typingIndicator: {
    display: "flex",
    alignItems: "center",
    gap: "2px",
    paddingLeft: "12px",
    paddingTop: "4px",
    "[dir='rtl'] &": {
      paddingLeft: 0,
      paddingRight: "12px",
    },
  },
  typingDot: {
    fontSize: "8px",
    color: "#999",
    animation: "$typingPulse 1.5s infinite ease-in-out",
    "&:nth-child(2)": {
      animationDelay: "0.3s",
    },
    "&:nth-child(3)": {
      animationDelay: "0.6s",
    },
  },
  "@keyframes typingPulse": {
    "0%, 60%, 100%": {
      opacity: 0.3,
    },
    "30%": {
      opacity: 1,
    },
  },
});
