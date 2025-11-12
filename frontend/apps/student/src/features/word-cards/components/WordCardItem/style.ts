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
      borderRadius: 14,
      padding: "10px 14px",
      minHeight: 100,
      boxShadow: "0 3px 10px rgba(0,0,0,0.04)",
      display: "flex",
      flexDirection: "column",
      justifyContent: "center",
      transition: "transform .1s ease, box-shadow .1s ease",
      "&:hover": {
        transform: "translateY(-2px)",
        boxShadow: "0 6px 16px rgba(0,0,0,0.08)",
      },
    },
    innerCard: {
      display: "flex",
      justifyContent: "space-between",
      alignItems: "center",
      gap: 14,
    },
    wordGroup: {
      maxWidth: "76%",
    },
    hebrew: {
      direction: "rtl",
      fontSize: 20,
      color: color.text,
      marginBottom: 2,
    },
    english: {
      fontSize: 16,
      color: color.textMuted,
      marginBottom: 2,
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
      gap: 8,
      "& .MuiCheckbox-root": { padding: 6 },
    },
    explanationContainer: {
      marginTop: 8,
    },
    explanationButton: {
      textTransform: "none",
      fontSize: "0.75rem",
    },
    explanationContent: {
      marginTop: 8,
      padding: 8,
      backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.06)`,
      borderRadius: 8,
    },
    explanationText: {
      fontSize: 14,
      color: color.textMuted,
    },
  })();
};
