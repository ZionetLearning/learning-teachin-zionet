import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  wrapper: {
    position: "relative",
    width: "300px",
    height: "300px",
    margin: "0 auto",
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
  },
});
