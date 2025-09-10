import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  chatWrapper: {
    maxWidth: "800px",
    height: "550px",
    margin: "0 auto",
    border: "1px solid #ccc",
    borderRadius: "10px",
    display: "flex",
    flexDirection: "column",
    position: "relative",
    overflowY: "auto",
  },

  mainContent: {
    flex: 1,
    display: "flex",
    flexDirection: "column",
    padding: "10px",
    transition: "margin-left 0.3s cubic-bezier(0.4, 0, 0.2, 1)",
    backgroundColor: "#fafafa",
  },

  mainContentShifted: {
    "@media (min-width: 769px)": {
      marginLeft: "320px",
    },
  },

  wrapper: {
    position: "relative",
    width: "220px",
    height: "220px",
    margin: "0 auto",
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
  avatar: {
    width: "100%",
    height: "100%",
    borderRadius: "50%",
    objectFit: "cover",
  },
});
