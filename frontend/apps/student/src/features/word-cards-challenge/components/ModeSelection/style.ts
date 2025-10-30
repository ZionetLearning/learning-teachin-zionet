import { createUseStyles } from "react-jss";
import { COLORS } from "../../constants";

const { PURPLE, PURPLE_DARK } = COLORS;

export const useStyles = createUseStyles({
  container: {
    minHeight: "100vh",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    padding: 24,
    boxSizing: "border-box",
    background:
      "linear-gradient(180deg, rgba(124,77,255,0.08) 0%, rgba(124,77,255,0.02) 100%)",
    "@media (prefers-color-scheme: dark)": {
      background:
        "linear-gradient(180deg, rgba(124,77,255,0.15) 0%, rgba(124,77,255,0.05) 100%)",
    },
  },
  modeSelection: {
    background: "white",
    borderRadius: 24,
    padding: 48,
    boxShadow:
      "0 14px 34px rgba(27, 18, 66, 0.18), 0 3px 10px rgba(0,0,0,0.08)",
    textAlign: "center",
    maxWidth: 500,
    width: "100%",
    "@media (prefers-color-scheme: dark)": {
      background: "#1e1e1e",
      boxShadow: "0 14px 34px rgba(0, 0, 0, 0.4), 0 3px 10px rgba(0,0,0,0.3)",
    },
  },
  title: {
    fontSize: 32,
    fontWeight: 700,
    color: PURPLE,
    marginBottom: 16,
  },
  subtitle: {
    fontSize: 18,
    color: "rgba(0,0,0,0.72)",
    marginBottom: 32,
    "@media (prefers-color-scheme: dark)": {
      color: "rgba(255,255,255,0.7)",
    },
  },
  modeButtons: {
    display: "flex",
    flexDirection: "column",
    gap: 16,
  },
  modeButton: {
    background: PURPLE,
    color: "#fff",
    fontSize: 18,
    fontWeight: 600,
    padding: "16px 32px",
    borderRadius: 16,
    boxShadow: "0 8px 18px rgba(124,77,255,0.28), 0 3px 8px rgba(0,0,0,0.12)",
    "&:hover": {
      background: PURPLE_DARK,
      boxShadow:
        "0 10px 22px rgba(124,77,255,0.34), 0 4px 10px rgba(0,0,0,0.14)",
    },
  },
});
