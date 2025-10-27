import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  headerWrapper: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    textAlign: "center",
    padding: 0,
    margin: "0 0 8px 0",
  },
  header: {
    color: "#7c4dff",
    fontSize: "2.4rem",
    fontWeight: 600,
    letterSpacing: "0.3px",
    margin: 0,
    textShadow: "0 1px 3px rgba(124,77,255,0.12)",
    "@media (max-width: 600px)": {
      fontSize: "1.9rem",
    },
  },
  underline: {
    marginTop: 6,
    width: 84,
    height: 4,
    borderRadius: 4,
    background: "linear-gradient(90deg, #7c4dff, #b388ff)",
  },
});
