import { createUseStyles } from "react-jss";

const PURPLE = "#7c4dff";
const PURPLE_DARK = "#5f35ff";
const GREEN = "#4caf50";
const RED = "#f44336";

export const useStyles = createUseStyles({
  container: {
    minHeight: "calc(100vh - 64px)",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    padding: 24,
    background:
      "linear-gradient(180deg, rgba(124,77,255,0.08) 0%, rgba(124,77,255,0.02) 100%)",
    "@media (prefers-color-scheme: dark)": {
      background:
        "linear-gradient(180deg, rgba(124,77,255,0.15) 0%, rgba(124,77,255,0.05) 100%)",
    },
  },
  centerState: {
    minHeight: 300,
    display: "flex",
    flexDirection: "column",
    gap: 16,
    alignItems: "center",
    justifyContent: "center",
  },
  emptyState: {
    minHeight: 300,
    display: "flex",
    flexDirection: "column",
    gap: 16,
    alignItems: "center",
    justifyContent: "center",
    textAlign: "center",
    padding: 24,
  },
  emptyTitle: {
    fontSize: 24,
    fontWeight: 700,
    color: PURPLE,
  },
  emptyDescription: {
    fontSize: 16,
    color: "rgba(0,0,0,0.62)",
    maxWidth: 500,
    "@media (prefers-color-scheme: dark)": {
      color: "rgba(255,255,255,0.7)",
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
  gameCard: {
    background: "white",
    borderRadius: 24,
    padding: 40,
    boxShadow:
      "0 14px 34px rgba(27, 18, 66, 0.18), 0 3px 10px rgba(0,0,0,0.08)",
    maxWidth: 600,
    width: "100%",
    animation: "$slideIn 0.4s ease-out",
    "@media (prefers-color-scheme: dark)": {
      background: "#1e1e1e",
      boxShadow: "0 14px 34px rgba(0, 0, 0, 0.4), 0 3px 10px rgba(0,0,0,0.3)",
    },
  },
  "@keyframes slideIn": {
    from: {
      opacity: 0,
      transform: "translateX(30px)",
    },
    to: {
      opacity: 1,
      transform: "translateX(0)",
    },
  },
  progressBar: {
    marginBottom: 24,
    textAlign: "center",
  },
  progressText: {
    fontSize: 14,
    fontWeight: 600,
    color: PURPLE,
  },
  questionBox: {
    background:
      "linear-gradient(180deg, rgba(124,77,255,0.10) 0%, rgba(124,77,255,0.03) 100%)",
    borderRadius: 16,
    padding: 32,
    marginBottom: 24,
    textAlign: "center",
    border: "2px solid rgba(124,77,255,0.25)",
    "@media (prefers-color-scheme: dark)": {
      background:
        "linear-gradient(180deg, rgba(124,77,255,0.20) 0%, rgba(124,77,255,0.08) 100%)",
      border: "2px solid rgba(124,77,255,0.4)",
    },
  },
  questionLabel: {
    fontSize: 14,
    fontWeight: 600,
    color: "rgba(0,0,0,0.62)",
    marginBottom: 12,
    "@media (prefers-color-scheme: dark)": {
      color: "rgba(255,255,255,0.7)",
    },
  },
  questionWord: {
    fontSize: 36,
    fontWeight: 700,
    color: "#23124a",
    fontFamily: "'Alef', sans-serif",
    "@media (prefers-color-scheme: dark)": {
      color: "#e0d0ff",
    },
  },
  answerBox: {
    display: "flex",
    flexDirection: "column",
    gap: 16,
  },
  textField: {
    "& .MuiOutlinedInput-root": {
      background: "#ffffff",
      borderRadius: 12,
      fontSize: 18,
      "& fieldset": {
        borderColor: "rgba(124,77,255,0.32)",
      },
      "&:hover fieldset": {
        borderColor: PURPLE,
      },
      "&.Mui-focused fieldset": {
        borderColor: PURPLE,
        borderWidth: 2,
      },
      "&.Mui-focused": {
        boxShadow: "0 0 0 2px rgba(124,77,255,0.16)",
      },
      "&.Mui-disabled": {
        background: "rgba(0,0,0,0.04)",
      },
      "@media (prefers-color-scheme: dark)": {
        background: "#2a2a2a",
        "&.Mui-disabled": {
          background: "rgba(255,255,255,0.08)",
        },
      },
    },
    "& .MuiInputBase-input": {
      color: "#000000",
      "@media (prefers-color-scheme: dark)": {
        color: "#ffffff",
      },
    },
    "& .MuiInputLabel-root.Mui-focused": {
      color: PURPLE,
    },
  },
  submitButton: {
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
    "&.Mui-disabled": {
      background: "rgba(124,77,255,0.35)",
      color: "rgba(255,255,255,0.92)",
      boxShadow: "none",
    },
  },
  feedbackCorrect: {
    marginTop: 16,
    padding: 16,
    borderRadius: 12,
    background: `linear-gradient(135deg, ${GREEN}15 0%, ${GREEN}08 100%)`,
    border: `2px solid ${GREEN}`,
    animation: "$fadeIn 0.3s ease-out",
  },
  feedbackWrong: {
    marginTop: 16,
    padding: 16,
    borderRadius: 12,
    background: `linear-gradient(135deg, ${RED}15 0%, ${RED}08 100%)`,
    border: `2px solid ${RED}`,
    animation: "$fadeIn 0.3s ease-out",
  },
  "@keyframes fadeIn": {
    from: {
      opacity: 0,
      transform: "translateY(-10px)",
    },
    to: {
      opacity: 1,
      transform: "translateY(0)",
    },
  },
  feedbackText: {
    fontSize: 16,
    fontWeight: 600,
    textAlign: "center",
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
