import { createUseStyles } from "react-jss";
export const useStyles = createUseStyles({
  gameContainer: {
    display: "flex",
    flexDirection: "row",
    justifyContent: "center",
  },
  speakersContainer: {
    display: "flex",
    flexDirection: "column",
    gap: "20px",
    alignItems: "center",
  },
  gameLogic: {
    display: "flex",
    flexDirection: "column",
    gap: "30px",
    paddingLeft: "0px",
    alignSelf: "center",
  },
  speakerAndButtonsWrapper: {
    display: "flex",
    flexDirection: "column",
    gap: "20px",
    alignItems: "center",
  },
});
