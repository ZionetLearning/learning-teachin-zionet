import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    tableWrapper: {
      width: "100%",
      display: "flex",
      justifyContent: "center",
      padding: 16,
    },

    paperWrapper: {
      width: "100%",
      maxWidth: 1100,
      borderRadius: 8,
      overflow: "hidden",
      backgroundColor: color.paper,
      border: `1px solid ${color.divider}`,
      boxShadow: `
        0 14px 34px rgba(0,0,0,0.18),
        0 3px 10px rgba(0,0,0,0.10)
      `,
      display: "flex",
      flexDirection: "column",
    },

    tableContainer: {
      maxHeight: "min(95vh, 700px)",
      overflowY: "auto",
    },

    // table header cells
    th: {
      fontWeight: 700,
      textAlign: "center",
      fontSize: 13,
      lineHeight: 1.4,
      color: color.text,
      backgroundColor: color.paper,
      borderBottom: `1px solid ${color.divider}`,
      backdropFilter: "blur(4px)",
    },

    // table body row
    tableRow: {
      backgroundColor: "transparent",
      borderBottom: `1px solid ${color.divider}`,
      "&:nth-of-type(odd)": {
        backgroundColor: "rgba(255,255,255,0.03)", // light tint in dark
      },
    },

    // table body cells
    td: {
      textAlign: "center",
      fontSize: 13,
      lineHeight: 1.4,
      color: color.text,
      paddingTop: 12,
      paddingBottom: 12,
    },

    lastAnswerBox: {
      maxWidth: 420,
      marginLeft: "auto",
      marginRight: "auto",
      whiteSpace: "nowrap",
      overflow: "hidden",
      textOverflow: "ellipsis",
      textAlign: "center",
      color: color.text,
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
  })();
};
