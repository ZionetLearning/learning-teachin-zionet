import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    roleBadge: {
      display: "inline-flex",
      alignItems: "center",
      justifyContent: "center",
      padding: "2px 10px",
      borderRadius: 999,
      fontSize: 12,
      fontWeight: 600,
      lineHeight: 1,
      letterSpacing: 0.3,
      border: "1px solid transparent",
      minWidth: 64,
      textTransform: "capitalize",
      userSelect: "none",
      transition: "background-color 0.2s, color 0.2s, border-color 0.2s",
    },

    roleStudent: {
      background: `rgba(var(${color.primaryMainChannel}) / 0.12)`,
      color: color.text,
      borderColor: `rgba(var(${color.primaryMainChannel}) / 0.35)`,
    },

    roleTeacher: {
      background:
        color.bg === "#fff" ? "rgba(16,185,129,0.10)" : "rgba(16,185,129,0.25)",
      color: color.text,
      borderColor:
        color.bg === "#fff" ? "rgba(16,185,129,0.35)" : "rgba(16,185,129,0.45)",
    },

    roleAdmin: {
      background:
        color.bg === "#fff" ? "rgba(239,68,68,0.10)" : "rgba(239,68,68,0.25)",
      color: color.text,
      borderColor:
        color.bg === "#fff" ? "rgba(239,68,68,0.35)" : "rgba(239,68,68,0.45)",
    },
  })();
};
