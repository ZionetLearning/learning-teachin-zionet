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
    },

    tableWrapper: {
      width: "100%",
      display: "flex",
      justifyContent: "center",
      padding: 16,
    },

    paperWrapper: {
      width: "100%",
      maxWidth: 1100,
      borderRadius: 16,
      overflow: "hidden",

      background: color.paper,
      border: `1px solid ${color.divider}`,
      boxShadow: `
        0 14px 34px rgba(0,0,0,0.18),
        0 3px 10px rgba(0,0,0,0.10)
      `,
      display: "flex",
      flexDirection: "column",
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
      marginBottom: 12,
      textAlign: "center",
      maxWidth: 620,
    },

    tableContainer: {
      maxHeight: "min(95vh, 700px)",
      overflowY: "auto",
    },

    // table header cells (Th)
    th: {
      fontWeight: 700,
      textAlign: "center",
      backdropFilter: "blur(4px)",
      backgroundColor: "rgba(255,255,255,0.08)", // fallback for dark
      background: `rgba(var(${color.primaryMainChannel}) / 0.08)`,
      color: color.text,
      fontSize: 14,
      lineHeight: 1.4,
    },

    // table body cells (Td)
    td: {
      textAlign: "center",
      fontSize: 14,
      lineHeight: 1.4,
      color: color.text,

      "&:nth-of-type(n)": {},
    },

    tableRow: {
      "&:nth-of-type(odd)": {
        background: "rgba(0,0,0,0.03)",
      },
    },

    lastAnswerBox: {
      maxWidth: 420,
      marginLeft: "auto",
      marginRight: "auto",
      whiteSpace: "nowrap",
      overflow: "hidden",
      textOverflow: "ellipsis",
      textAlign: "center",
      color: color.textMuted,
      fontSize: 13,
      lineHeight: 1.4,
    },

    tablePaginationWrapper: {
      position: "fixed",
      bottom: 0,
      left: 0,
      right: 0,
      backgroundColor: color.paper,
      color: color.text,
      borderTop: `1px solid ${color.divider}`,
      backdropFilter: "blur(6px)",
    },

    retryButton: {
      textTransform: "none",
      borderRadius: 999,
      fontWeight: 600,
      padding: "6px 12px",
      backgroundColor: color.primary,
      color: color.primaryContrast,
      boxShadow: `0 8px 18px rgba(var(${color.primaryMainChannel}) / 0.28),
                  0 3px 8px rgba(0,0,0,0.12)`,

      "&:hover": {
        background: color.primary,
        boxShadow: `0 10px 22px rgba(var(${color.primaryMainChannel}) / 0.34),
                    0 4px 10px rgba(0,0,0,0.14)`,
        filter: "brightness(1.05)",
      },
    },
  })();
};
