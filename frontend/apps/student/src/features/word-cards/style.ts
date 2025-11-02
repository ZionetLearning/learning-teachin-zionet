import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    body: {
      marginTop: 8,
    },

    centerState: {
      minHeight: 220,
      display: "flex",
      flexDirection: "column",
      gap: 12,
      alignItems: "center",
      justifyContent: "center",
    },

    grid: {
      display: "grid",
      gridTemplateColumns: "repeat(auto-fill, minmax(260px, 1fr))",
      gap: 15,
      padding: "2%",
    },

    headerWrapper: {
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      textAlign: "center",
      background: `linear-gradient(
        180deg,
        rgba(var(${color.primaryMainChannel}) / 0.06) 0%,
        rgba(var(${color.primaryMainChannel}) / 0.02) 100%
      )`,
      padding: "24px 16px 8px",
      marginBottom: 16,
      borderBottom: `1px solid ${color.divider}`,
    },

    title: {
      color: color.primary,
      fontSize: 26,
      fontWeight: 700,
      letterSpacing: 0.2,
      marginBottom: 8,
    },

    description: {
      color: color.text,
      fontSize: 16,
      opacity: 0.9,
      maxWidth: 900,
    },

    helperNote: {
      marginTop: 8,
      color: color.textMuted,
    },

    headerActions: {
      marginTop: 12,
      display: "flex",
      gap: 12,
      alignItems: "center",
      "&[dir='rtl']": { flexDirection: "row-reverse" },
    },

    addCardBtn: {
      borderRadius: 12,
      padding: "8px 16px",
      background: color.primary,
      color: color.primaryContrast,
      boxShadow: `0 8px 18px rgba(var(${color.primaryMainChannel}) / 0.28), 0 3px 8px rgba(0,0,0,0.12)`,

      "&:hover": {
        background: color.primaryDark,
        boxShadow: `0 10px 22px rgba(var(${color.primaryMainChannel}) / 0.34), 0 4px 10px rgba(0,0,0,0.14)`,
      },

      "& .MuiButton-startIcon": { marginInlineEnd: 8 },
      "& .MuiButton-endIcon": { marginInlineStart: 8 },

      "&[dir='rtl'] .MuiButton-startIcon": {
        marginLeft: 8,
        marginRight: 0,
      },
      "&[dir='rtl'] .MuiButton-endIcon": {
        marginRight: 8,
        marginLeft: 0,
      },
    },
    practiceBtn: {
      color: color.primary,
      borderColor: color.primary,
      borderRadius: 12,
      padding: "8px 16px",
      fontWeight: 600,
      "&:hover": {
        borderColor: color.primaryDark,
        background: "rgba(124,77,255,0.08)",
      },
      "& .MuiButton-startIcon": {
        marginInlineEnd: 8,
      },
      "& .MuiButton-endIcon": {
        marginInlineStart: 8,
      },
      "&[dir='rtl'] .MuiButton-startIcon": {
        marginLeft: 8,
        marginRight: 0,
      },
      "&[dir='rtl'] .MuiButton-endIcon": {
        marginRight: 8,
        marginLeft: 0,
      },
    },
    actionButtons: {
      display: "flex",
      gap: 8,
    },
  })();
};
