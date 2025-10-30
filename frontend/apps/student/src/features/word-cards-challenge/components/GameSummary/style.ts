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
  summary: {
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
  summaryTitle: {
    fontSize: 28,
    fontWeight: 700,
    color: PURPLE,
    marginBottom: 32,
  },
  scoreBox: {
    background:
      "linear-gradient(180deg, rgba(124,77,255,0.10) 0%, rgba(124,77,255,0.03) 100%)",
    borderRadius: 16,
    padding: 32,
    marginBottom: 32,
    border: "2px solid rgba(124,77,255,0.25)",
    "@media (prefers-color-scheme: dark)": {
      background:
        "linear-gradient(180deg, rgba(124,77,255,0.20) 0%, rgba(124,77,255,0.08) 100%)",
      border: "2px solid rgba(124,77,255,0.4)",
    },
  },
  scoreText: {
    fontSize: 16,
    fontWeight: 600,
    color: "rgba(0,0,0,0.62)",
    marginBottom: 8,
    "@media (prefers-color-scheme: dark)": {
      color: "rgba(255,255,255,0.7)",
    },
  },
  scoreNumber: {
    fontSize: 56,
    fontWeight: 700,
    color: PURPLE,
    marginBottom: 8,
  },
  scoreDetails: {
    fontSize: 14,
    color: "rgba(0,0,0,0.62)",
    "@media (prefers-color-scheme: dark)": {
      color: "rgba(255,255,255,0.7)",
    },
  },
  summaryButtons: {
    display: "flex",
    flexDirection: "column",
    gap: 12,
  },
  summaryButton: {
    background: PURPLE,
    color: "#fff",
    fontSize: 16,
    fontWeight: 600,
    padding: "12px 24px",
    borderRadius: 12,
    boxShadow: "0 8px 18px rgba(124,77,255,0.28), 0 3px 8px rgba(0,0,0,0.12)",
    "&:hover": {
      background: PURPLE_DARK,
      boxShadow:
        "0 10px 22px rgba(124,77,255,0.34), 0 4px 10px rgba(0,0,0,0.14)",
    },
  },
  summaryButtonOutlined: {
    color: PURPLE,
    borderColor: PURPLE,
    fontSize: 16,
    fontWeight: 600,
    padding: "12px 24px",
    borderRadius: 12,
    "&:hover": {
      borderColor: PURPLE_DARK,
      background: "rgba(124,77,255,0.08)",
    },
  },
});
