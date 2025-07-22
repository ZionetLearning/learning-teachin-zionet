import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  wrapper: {
    position: "relative",
    width: "300px",
    height: "300px",
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
});
