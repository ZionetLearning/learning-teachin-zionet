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
    summary: {
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
    summaryTitle: {
      fontSize: 28,
      fontWeight: 700,
      color: color.primary,
      marginBottom: 32,
    },
    scoreBox: {
      background: `linear-gradient(
      180deg,
      rgba(var(${color.primaryMainChannel}) / 0.10) 0%,
      rgba(var(${color.primaryMainChannel}) / 0.03) 100%
    )`,
      borderRadius: 16,
      padding: 32,
      marginBottom: 32,
      border: `2px solid rgba(var(${color.primaryMainChannel}) / 0.30)`,
    },
    scoreText: {
      fontSize: 16,
      fontWeight: 600,
      color: color.textMuted,
      marginBottom: 8,
    },
    scoreNumber: {
      fontSize: 56,
      fontWeight: 700,
      color: color.primary,
      marginBottom: 8,
    },
    scoreDetails: {
      fontSize: 14,
      color: color.textMuted,
    },
    summaryButtons: {
      display: "flex",
      flexDirection: "column",
      gap: 12,
    },
    summaryButton: {
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
    },
    summaryButtonOutlined: {
      color: color.primary,
      borderColor: color.primary,
      fontSize: 16,
      fontWeight: 600,
      padding: "12px 24px",
      borderRadius: 12,
      "&:hover": {
        borderColor: color.primaryDark,
        background: `rgba(var(${color.primaryMainChannel}) / 0.08)`,
      },
    },
  })();
};
