import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";
import { COLORS } from "./constants";

const { GREEN, RED } = COLORS;

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    container: {
      minHeight: "100vh",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      padding: 24,
      boxSizing: "border-box",
      background: color.bg,
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
      color: color.primary,
    },
    emptyDescription: {
      fontSize: 16,
      color: color.textMuted,
      maxWidth: 500,
    },
    gameCard: {
      background: color.paper,
      borderRadius: 24,
      padding: 40,
      boxShadow:
        "0 8px 24px rgba(0, 0, 0, 0.12), 0 2px 6px rgba(0, 0, 0, 0.08)",
      maxWidth: 600,
      width: "100%",
      animation: "$slideIn 0.4s ease-out",
      border: `1px solid ${color.divider}`,
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
      color: color.primary,
    },
    questionBox: {
      background: `linear-gradient(
      180deg,
      rgba(var(${color.primaryMainChannel}) / 0.10) 0%,
      rgba(var(${color.primaryMainChannel}) / 0.03) 100%
    )`,
      borderRadius: 16,
      padding: 32,
      marginBottom: 24,
      textAlign: "center",
      border: `2px solid rgba(var(${color.primaryMainChannel}) / 0.30)`,
    },
    questionLabel: {
      fontSize: 14,
      fontWeight: 600,
      color: color.textMuted,
      marginBottom: 12,
    },
    questionWord: {
      fontSize: 36,
      fontWeight: 700,
      color: color.text,
      fontFamily: "'Alef', sans-serif",
    },
    answerBox: {
      display: "flex",
      flexDirection: "column",
      gap: 16,
    },
    textField: {
      "& .MuiOutlinedInput-root": {
        background: color.bg,
        borderRadius: 12,
        fontSize: 18,
        "& fieldset": {
          borderColor: color.divider,
        },
        "&:hover fieldset": {
          borderColor: color.primary,
        },
        "&.Mui-focused fieldset": {
          borderColor: color.primary,
          borderWidth: 2,
        },
        "&.Mui-focused": {
          boxShadow: `0 0 0 2px rgba(var(${color.primaryMainChannel}) / 0.16)`,
        },
        "&.Mui-disabled": {
          background: color.divider,
        },
      },
      "& .MuiInputBase-input": {
        color: color.text,
      },
      "& .MuiInputLabel-root.Mui-focused": {
        color: color.primary,
      },
    },
    submitButton: {
      background: color.primary,
      color: color.primaryContrast,
      fontSize: 16,
      fontWeight: 600,
      padding: "12px 24px",
      borderRadius: 12,
      boxShadow: `0 8px 18px rgba(var(${color.primaryMainChannel}) / 0.28), 0 3px 8px rgba(0,0,0,0.12)`,
      "&:hover": {
        background: color.primaryDark,
        boxShadow: `0 10px 22px rgba(var(${color.primaryMainChannel}) / 0.34), 0 4px 10px rgba(0,0,0,0.14)`,
      },
      "&.Mui-disabled": {
        background: `rgba(var(${color.primaryMainChannel}) / 0.35)`,
        color: color.primaryContrast,
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
  })();
};
