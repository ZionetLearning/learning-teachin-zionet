import { createUseStyles } from "react-jss";
import { COLORS } from "./constants";

const { PURPLE, PURPLE_DARK, GREEN, RED } = COLORS;

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
  "@keyframes slideOutRight": {
    "0%": {
      transform: "translateX(0) rotate(0deg)",
      opacity: 1,
    },
    "100%": {
      transform: "translateX(100%) rotate(15deg)",
      opacity: 0,
    },
  },
  "@keyframes slideOutLeft": {
    "0%": {
      transform: "translateX(0) rotate(0deg)",
      opacity: 1,
    },
    "100%": {
      transform: "translateX(-100%) rotate(-15deg)",
      opacity: 0,
    },
  },
  gameCardSlideRight: {
    animation: "$slideOutRight 0.5s ease-in forwards",
  },
  gameCardSlideLeft: {
    animation: "$slideOutLeft 0.5s ease-in forwards",
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
  feedbackModal: {
    "& .MuiBackdrop-root": {
      backgroundColor: "rgba(0, 0, 0, 0.7)",
    },
  },
  feedbackModalCorrect: {
    background: `linear-gradient(135deg, ${GREEN} 0%, ${GREEN}dd 100%)`,
    borderRadius: 24,
    padding: "48px 64px",
    boxShadow: `0 20px 60px ${GREEN}80`,
    animation: "$scaleIn 0.3s ease-out",
  },
  feedbackModalWrong: {
    background: `linear-gradient(135deg, ${RED} 0%, ${RED}dd 100%)`,
    borderRadius: 24,
    padding: "48px 64px",
    boxShadow: `0 20px 60px ${RED}80`,
    animation: "$scaleIn 0.3s ease-out",
  },
  "@keyframes scaleIn": {
    from: {
      opacity: 0,
      transform: "scale(0.8)",
    },
    to: {
      opacity: 1,
      transform: "scale(1)",
    },
  },
  feedbackModalContent: {
    textAlign: "center",
  },
  feedbackModalText: {
    fontSize: 32,
    fontWeight: 700,
    color: "#ffffff",
    textShadow: "0 2px 8px rgba(0,0,0,0.2)",
  },
});
