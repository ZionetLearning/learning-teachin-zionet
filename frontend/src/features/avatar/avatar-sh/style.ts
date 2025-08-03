import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({

  chatWrapper: {
    maxWidth: "800px",
    maxHeight: "550px",
    margin: "auto",
    marginTop: 0,
    marginBottom: 0,
    border: "1px solid #ccc",
    padding: 10,
    overflowY: "auto",
    display: "flex",
    flexDirection: "column"
  },
  wrapper: {
    position: "relative",
    width: "220px",
    height: "220px",
    margin: "0 auto",
    //overflowY: "auto"
  },
  messagesList: {
    //height: "180px",
    //overflowX: "auto",
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
  inputContainer: {
    position: "relative",
    flexShrink: 0,
    padding: 6,
  },

  input: {

    /*fontSize: "18px",
    //padding: "10px",
    width: "100% !important",
    boxSizing: "border-box",
    borderRadius: 5,
    border: "1px solid #ccc",*/

    /*border: "1px solid #ddd",
    borderRadius: "0%",
    paddingLeft: "6px",
    boxSizing: "border-box"*/

    width: "100%!important",
    fontSize: "18px",
    padding: "1px 6px 1px 6px", // room for buttons
    border: "1px solid #ccc",
    borderRadius: 5,
    boxSizing: "border-box",

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
