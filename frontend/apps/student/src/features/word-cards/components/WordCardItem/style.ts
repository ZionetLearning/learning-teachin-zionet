import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

const GREEN = "#2bbd7e";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    card: {
      position: "relative",
      background: `linear-gradient(${color.paper}, ${color.paper}) padding-box,
        linear-gradient(
          180deg,
          rgba(var(${color.primaryMainChannel}) / 0.25),
          rgba(var(${color.primaryMainChannel}) / 0.05)
        ) border-box`,
      border: "1px solid transparent",
      borderRadius: 16,
      padding: 0,
      overflow: "hidden",
      boxShadow: "0 2px 8px rgba(0,0,0,0.06)",
      transition: "transform .2s ease, box-shadow .2s ease",
      "&:hover": {
        transform: "translateY(-3px)",
        boxShadow: "0 8px 20px rgba(0,0,0,0.12)",
      },
    },
    cardContent: {
      display: "flex",
      flexDirection: "column",
      gap: 0,
    },
    topRow: {
      display: "flex",
      justifyContent: "space-between",
      alignItems: "center",
      gap: 16,
      padding: "16px 18px",
      minHeight: 100,
    },
    wordGroup: {
      flex: 1,
      minWidth: 0,
    },
    hebrew: {
      direction: "rtl",
      fontSize: 22,
      fontWeight: 600,
      color: color.text,
      marginBottom: 6,
      lineHeight: 1.3,
    },
    english: {
      fontSize: 16,
      color: color.textMuted,
      lineHeight: 1.4,
    },
    learnControl: {
      alignSelf: "flex-end",
      marginTop: 4,
      "& .MuiFormControlLabel-label": {
        fontSize: 13,
        fontWeight: 600,
        color: color.textMuted,
      },
    },
    learnLabel: {
      fontWeight: 600,
      fontSize: 13,
      color: color.text,
      userSelect: "none",
    },
    learnIconActive: {
      color: GREEN,
      filter: "drop-shadow(0 2px 6px rgba(43,189,126,0.35))",
    },
    learnIconIdle: {
      color: color.divider,
    },
    learnRow: {
      display: "inline-flex",
      alignItems: "center",
      gap: 6,
      flexShrink: 0,
      "& .MuiCheckbox-root": { padding: 6 },
    },
    definitionSection: {
      borderTop: `1px solid ${color.divider}`,
      backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.02)`,
    },
    definitionToggle: {
      textTransform: "none",
      fontSize: "0.8125rem",
      fontWeight: 500,
      color: color.primary,
      padding: "10px 18px",
      justifyContent: "space-between",
      borderRadius: 0,
      transition: "background-color .2s ease",
      "&:hover": {
        backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.06)`,
      },
      "&:focus": {
        outline: "none",
        boxShadow: "none",
      },
      "&:focus-visible": {
        outline: "none",
        boxShadow: "none",
      },
      "& .MuiButton-endIcon": {
        marginLeft: "auto",
        marginRight: -4,
      },
    },
    definitionContent: {
      padding: "0 18px 16px 18px",
      animation: "$fadeIn 0.3s ease-in-out",
    },
    definitionText: {
      fontSize: 14,
      lineHeight: 1.6,
      color: color.textMuted,
      padding: "12px 16px",
      backgroundColor: color.paper,
      borderRadius: 10,
      border: `1px solid ${color.divider}`,
      boxShadow: "0 1px 3px rgba(0,0,0,0.04)",
    },
    "@keyframes fadeIn": {
      from: {
        opacity: 0,
        transform: "translateY(-8px)",
      },
      to: {
        opacity: 1,
        transform: "translateY(0)",
      },
    },
  })();
};
