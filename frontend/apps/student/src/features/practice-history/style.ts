import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    headerWrapper: {
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      textAlign: "center",
      background: `linear-gradient(
        180deg,
        rgba(var(${color.primaryMainChannel}) / 0.10) 0%,
        rgba(var(${color.primaryMainChannel}) / 0.03) 100%
      )`,
      padding: 16,
      borderBottom: `1px solid ${color.divider}`,
    },

    title: {
      color: color.text,
      fontSize: 26,
      fontWeight: 700,
      letterSpacing: 0.2,
      lineHeight: 1.2,
    },

    description: {
      color: color.text,
      fontSize: 16,
      fontWeight: 400,
      opacity: 0.9,
      lineHeight: 1.4,
      marginTop: 4,
    },

    toggleGroupWrapper: {
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      gap: 12,
      marginTop: 16,
      background: `
        linear-gradient(
          180deg,
          rgba(var(${color.primaryMainChannel}) / 0.12) 0%,
          rgba(var(${color.primaryMainChannel}) / 0.05) 100%
        )
      `,
      border: `1px solid rgba(var(${color.primaryMainChannel}) / 0.3)`,
      borderRadius: 50,
      padding: 6,
      backdropFilter: "blur(6px)",
      boxShadow: `
        0 6px 20px rgba(var(${color.primaryMainChannel}) / 0.32),
        0 2px 6px rgba(0,0,0,0.4)
      `,
    },

    toggleGroup: {
      "& .MuiToggleButton-root": {
        // base / unselected
        color: color.text, // was color.primary
        background: "transparent", // was rgba(primaryMainChannel / 0.08)
        margin: "0 6px",
        border: "none",
        textTransform: "none",
        fontWeight: 600,
        borderRadius: 999,
        transition: "all 0.25s ease",
        padding: "6px 20px",

        "&:hover": {
          background: `rgba(var(${color.primaryMainChannel}) / 0.12)`,
        },
      },

      "& .Mui-selected": {
        color: `${color.primaryContrast} !important`,
        backgroundColor: `${color.primary} !important`,
        boxShadow: `
      0 2px 8px rgba(var(${color.primaryMainChannel}) / 0.4),
      0 1px 2px rgba(0,0,0,0.5)
    `,
        "&:hover": {
          backgroundColor: `${color.primaryDark} !important`,
          boxShadow: `
        0 2px 10px rgba(var(${color.primaryMainChannel}) / 0.6),
        0 2px 4px rgba(0,0,0,0.6)
      `,
        },
      },
    },
  })();
};
