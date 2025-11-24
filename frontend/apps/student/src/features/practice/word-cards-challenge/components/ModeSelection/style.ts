import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

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
    modeSelection: {
      background: color.paper,
      borderRadius: 24,
      padding: 48,
      boxShadow:
        "0 8px 24px rgba(0, 0, 0, 0.12), 0 2px 6px rgba(0, 0, 0, 0.08)",
      textAlign: "center",
      maxWidth: 500,
      width: "100%",
      border: `1px solid ${color.divider}`,
    },
    title: {
      fontSize: 32,
      fontWeight: 700,
      color: color.primary,
      marginBottom: 16,
    },
    subtitle: {
      fontSize: 18,
      color: color.textMuted,
      marginBottom: 32,
    },
    modeButtons: {
      display: "flex",
      flexDirection: "column",
      gap: 16,
    },
    modeButton: {
      background: color.primary,
      color: color.primaryContrast,
      fontSize: 18,
      fontWeight: 600,
      padding: "16px 32px",
      borderRadius: 16,
      boxShadow: `0 8px 18px rgba(var(${color.primaryMainChannel}) / 0.28), 0 3px 8px rgba(0,0,0,0.12)`,
      "&:hover": {
        background: color.primaryDark,
        boxShadow: `0 10px 22px rgba(var(${color.primaryMainChannel}) / 0.34), 0 4px 10px rgba(0,0,0,0.14)`,
      },
    },
  })();
};
