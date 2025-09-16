import { createUseStyles } from "react-jss";
export const useStyles = createUseStyles({
  gameContainer: {
    display: "flex",
    flexDirection: "row",
    justifyContent: "center",
    gap: "100px",
  },
  speakersContainer: {
    display: "flex",
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    gap: "10px",
  },
  gameLogic: {
    display: "flex",
    flexDirection: "column",
    gap: "40px",
    paddingLeft: "0px",
    alignSelf: "center",
  },
});
