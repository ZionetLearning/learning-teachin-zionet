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
  },
  messagesListAvatar: {
    flex: 1,
    overflowY: "auto",
    overflowX: "hidden",
    marginBottom: 10,
  },
  messageBox: {
    "& .rce-mbox-right-notch": {
      fill: "#11bbff !important",
    },
    "& .rce-container-mbox-right": {
      flexDirection: "row-reverse",
    },
    "& .rce-mbox-right .rce-mbox-title": {
      textAlign: "right",
      justifyContent: "flex-end",
    },
    "& svg": {
      display: "none !important",
    },
  },
  inputContainer: {
    flexShrink: 0,
    padding: 6,
  },
  input: {
    border: "1px solid #ddd",
    borderRadius: "0%",
    paddingLeft: "6px",
    "& input": {
      color: "black",
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
    "& input": {
      color: "black",
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
});
