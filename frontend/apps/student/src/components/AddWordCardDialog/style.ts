import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    dialogPaper: {
      background: color.paper,
      borderRadius: 16,
      border: `1px solid ${color.divider}`,
      boxShadow: `
      0 14px 34px rgba(0,0,0,0.18),
      0 3px 10px rgba(0,0,0,0.10)
    `,
      overflow: "hidden",
    },

    dialogBodyGradient: {
      background: `linear-gradient(
      180deg,
      rgba(var(${color.primaryMainChannel}) / 0.08) 0%,
      rgba(var(${color.primaryMainChannel}) / 0.03) 100%
    )`,
      padding: 16,
      borderRadius: 16,
    },

    dialogTitle: {
      padding: "16px 56px 8px 24px",
      color: color.text,
    },

    closeButton: {
      position: "absolute",
      right: 8,
      top: 8,
      color: color.primary,
      "&:hover": {
        background: `rgba(var(${color.primaryMainChannel}) / 0.10)`,
      },
    },

    wordPanel: {
      marginBottom: 16,
      padding: 5,
      borderRadius: 12,
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      justifyContent: "center",
      textAlign: "center",
      background: `rgba(var(${color.primaryMainChannel}) / 0.06)`,
      border: `1px solid rgba(var(${color.primaryMainChannel}) / 0.25)`,
      fontFamily: "'Alef', sans-serif",
    },

    wordMeta: { display: "flex", flexDirection: "column" },

    wordLabel: {
      fontSize: 12,
      fontWeight: 600,
      color: color.textMuted,
      marginBottom: 4,
    },

    hebrewWord: {
      fontSize: 20,
      lineHeight: 1.25,
      color: color.text,
    },

    textField: {
      "& .MuiOutlinedInput-root": {
        background: color.paper,
        borderRadius: 12,
        "& .MuiOutlinedInput-notchedOutline": { borderColor: color.divider },
        "&:hover .MuiOutlinedInput-notchedOutline": {
          borderColor: color.divider,
        },
        "&.Mui-focused .MuiOutlinedInput-notchedOutline": {
          borderColor: color.primary,
        },
        "&.Mui-focused": {
          boxShadow: `0 0 0 2px rgba(var(${color.primaryMainChannel}) / 0.16)`,
        },
      },
      "& .MuiInputLabel-root": {
        color: color.textMuted,
        fontWeight: 600,
      },
      "& .MuiInputLabel-root.Mui-focused": { color: color.primary },
    },

    actions: {
      padding: "0 24px 16px 24px",
      display: "flex",
      gap: 12,
      justifyContent: "flex-end",
    },

    cancelButton: {
      color: color.primary,
      fontWeight: 700,
      padding: "8px 12px",
      borderRadius: 10,
      "&:hover": {
        background: `rgba(var(${color.primaryMainChannel}) / 0.10)`,
      },
    },

    saveButton: {
      borderRadius: 12,
      padding: "8px 16px",
      boxShadow: `0 8px 18px rgba(var(${color.primaryMainChannel}) / 0.28), 0 3px 8px rgba(0,0,0,0.12)`,
      "&:hover": {
        boxShadow: `0 10px 22px rgba(var(${color.primaryMainChannel}) / 0.34), 0 4px 10px rgba(0,0,0,0.14)`,
      },
      "&.Mui-disabled": {
        background: `rgba(var(${color.primaryMainChannel}) / 0.35)`,
        color: color.primaryContrast,
        boxShadow: "none",
      },
    },
  })();
};
