import { createUseStyles } from "react-jss";
export const useStyles = createUseStyles({
  welcomeContainer: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "start",
    minHeight: "60vh",
    textAlign: "center",
    gap: "24px",
  },
  welcomeText: {
    maxWidth: 600,
    paddingTop: 64,
    fontSize: 18,
  },
  welcomeButton: {
    minWidth: 200,
  },
});
