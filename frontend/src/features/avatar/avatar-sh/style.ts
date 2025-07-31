import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({

  chatWrapper: {
    maxWidth: "800px",
    maxHeight: "550px",
    margin: "auto",
    border: "1px solid #ccc",
    padding: 10,
  },
  wrapper: {
    position: "relative",
    width: "300px",
    height: "300px",
    margin: "0 auto",
  },
  messagesList: {
    height: "280px",
    overflowX: "auto",
    overflowY: "auto",
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
  rightButtons: {
    display: "flex",
    flexDirection: "row",
    gap: "5px"
  },
  button: {
    marginTop: "10px",
    background: "#59BEDFFF",
    color: "white",
    padding: [10, 20],
    border: "none",
    borderRadius: 15,
    cursor: "pointer",
    "&:hover": {
      background: "#1B81A6FF",
    },
  },
  buttonRed: {
    marginTop: "10px",
    background: "red",
    color: "white",
    padding: [10, 20],
    border: "none",
    borderRadius: 15,
    cursor: "pointer",
  },
  lipsImage: {
    position: "absolute",
    top: "42%",
    left: "40%",
    width: "20%",
    height: "20%",
    pointerEvents: "none",
  },
  input: {
    fontSize: "18px",
    padding: "10px",
    width: "300px",
    borderRadius: 5,
    border: "1px solid #ccc",
  },
  avatar: {
    width: "100%",
    height: "100%",
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
