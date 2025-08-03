import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  chatWrapper: {
    maxWidth: "800px",
    maxHeight: "800px",
    margin: "auto",
    border: "1px solid #ccc",
    padding: 10,
  },
  chatWrapperAvatar: {
    maxWidth: "800px",
    maxHeight: "550px",
    margin: "auto",
    marginTop: 0,
    marginBottom: 0,
    border: "1px solid #ccc",
    padding: 10,
    overflowY: "auto",
    display: "flex",
    flexDirection: "column",
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
  },
  inputContainer: {
    position: "relative",
    flexShrink: 0,
    padding: 6,
  },
  input: {
    border: "1px solid #ddd",
    borderRadius: "0%",
    paddingLeft: "6px",
  },
  inputAvatar: {
    width: "100% !important",
    fontSize: "18px",
    padding: "1px 6px 1px 6px",
    border: "1px solid #ccc",
    borderRadius: 5,
    boxSizing: "border-box",
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
