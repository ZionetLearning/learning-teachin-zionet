import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  descriptionWrapper: {
    display: "flex",
    justifyContent: "center",
    textAlign: "center",
    padding: 0,
    marginBottom: 12,
  },
  description: {
    color: "#6f42c1",
    fontSize: "1.1rem",
    fontWeight: 500,
    lineHeight: 1.5,
    maxWidth: 640,
    margin: 0,
    letterSpacing: 0.2,
    textShadow: "0 1px 2px rgba(124,77,255,0.08)",
    "@media (max-width: 600px)": {
      fontSize: "1rem",
      lineHeight: 1.45,
    },
  },
});
