import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    tableArea: {
      flex: 1,
      display: "flex",
      flexDirection: "column",
      minHeight: 0,
    },

    tableShell: {
      flex: 1,
      display: "flex",
      flexDirection: "column",
      minHeight: 0,
      border: `1px solid ${color.divider}`, 
      borderRadius: 8,
      background: color.paper, 
    },

    rowsScroll: {
      flex: 1,
      minHeight: 0,
      overflowY: "auto",
      overflowX: "auto",
      WebkitOverflowScrolling: "touch",
      scrollbarGutter: "stable",
    },

    table: {
      tableLayout: "fixed",
      width: "100%",
      borderCollapse: "separate",
      borderSpacing: 0,

      "& .MuiTableCell-head": {
        textAlign: "inherit",
        background: color.bg, 
        fontWeight: 600,
        color: color.text, 
        whiteSpace: "nowrap",
        position: "sticky",
        top: 0,
        zIndex: 1,
        boxSizing: "border-box",
        verticalAlign: "middle",
      },

      "& .MuiTableBody-root .MuiTableCell-root": {
        textAlign: "center",
        whiteSpace: "nowrap",
        verticalAlign: "top",
        boxSizing: "border-box",
        color: color.text, 
      },

      "@media (max-width: 600px)": {
        "& .MuiTableCell-root": {
          padding: "6px 8px",
          fontSize: 13,
        },
      },
    },

    tableWide: {
      "@media (max-width: 600px)": {
        minWidth: 700,
      },
    },

    colStudent: {
      width: "72%",
      "@media (max-width: 900px)": { width: "65%" },
      "@media (max-width: 600px)": { width: "58%" },
    },

    colMetrics: {
      width: "28%",
      "@media (max-width: 900px)": { width: "35%" },
      "@media (max-width: 600px)": { width: "42%" },
    },

    cap: { textTransform: "capitalize" },

    ellipsis: {
      display: "block",
      maxWidth: "100%",
      overflow: "hidden",
      textOverflow: "ellipsis",
      whiteSpace: "nowrap",
      color: color.text, 
    },

    rateWrapper: {
      display: "flex",
      alignItems: "center",
      gap: 8,
      justifyContent: "center",
    },

    rateBarWrap: {
      width: 140,
      "@media (max-width: 600px)": { width: 90 },
    },

    rateBar: {
      height: 8,
      borderRadius: 6,
      "& .MuiLinearProgress-bar": { borderRadius: 6 },
    },

    rateText: {
      fontSize: 12,
      color: color.textMuted, 
    },

    groupCell: {
      padding: 0,
      borderBottom: "none",
    },

    groupAccordion: {
      borderRadius: 10,
      border: `1px solid ${color.divider}`, 
      overflow: "hidden",
      margin: "6px 0",
      boxShadow: "0 2px 6px rgba(0,0,0,0.04)",
      "&::before": { display: "none" },
      background: "transparent", 
    },

    groupSummary: {
      padding: "10px 12px",
      background: color.bg, 
      "& .MuiAccordionSummary-content": {
        margin: 0,
        width: "100%",
        color: color.text, 
      },
    },

    groupDetails: {
      padding: "10px 12px 12px",
      background: color.paper, 
      color: color.text,
    },

    summaryBar: {
      display: "grid",
      alignItems: "center",
      gap: 12,
      width: "100%",
      minHeight: 32,
      gridTemplateColumns: "72% 28%",
      "@media (max-width: 900px)": {
        gridTemplateColumns: "65% 35%",
      },
      "@media (max-width: 600px)": {
        gridTemplateColumns: "58% 42%",
      },
      "@media (max-width: 480px)": {
        gridTemplateColumns: "1fr",
        gridAutoRows: "auto",
        alignItems: "start",
        rowGap: 8,
      },
    },

    summaryLeft: {
      minWidth: 0,
      overflow: "hidden",
      display: "flex",
      alignItems: "center",
      gap: 8,
      fontWeight: 600,
      color: color.text,
    },

    studentId: {
      fontFamily:
        "ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace",
      color: color.text, 
    },

    summaryRight: {
      display: "flex",
      alignItems: "center",
      gap: 10,
      rowGap: 8,
      flexWrap: "nowrap",
      justifySelf: "end",
      overflowX: "auto",
      overflowY: "hidden",
      whiteSpace: "nowrap",
      "& strong": { fontWeight: 700, color: color.text },
      scrollbarGutter: "stable",
      "-webkit-overflow-scrolling": "touch",
      paddingRight: 10,
      "@media (max-width: 480px)": {
        justifySelf: "start",
        paddingRight: 0,
      },
      color: color.text,
    },

    metric: {
      padding: "4px 8px",
      border: `1px solid ${color.divider}`, 
      borderRadius: 8,
      background: color.paper, 
      fontSize: 13,
      flex: "0 0 auto",
      color: color.text,
    },

    ratePill: {
      padding: "4px 10px",
      borderRadius: 999,
      background: `rgba(var(${color.primaryMainChannel}) / 0.08)`, 
      border: `1px solid rgba(var(${color.primaryMainChannel}) / 0.25)`, 
      fontWeight: 700,
      fontVariantNumeric: "tabular-nums",
      flex: "0 0 auto",
      color: color.text, 
    },

    innerTable: {
      border: `1px solid ${color.divider}`, 
      borderRadius: 8,
      overflow: "hidden",
      tableLayout: "fixed",
      width: "100%",
      background: color.paper,

      "& .MuiTableCell-head": {
        background: color.bg, // was #f9fafb
        textAlign: "center",
        boxSizing: "border-box",
        verticalAlign: "middle",
        color: color.text,
      },

      "& .MuiTableBody-root .MuiTableCell-root": {
        textAlign: "center",
        boxSizing: "border-box",
        verticalAlign: "middle",
        color: color.text,
      },
    },

    colInnerGameType: { width: "22%" },
    colInnerDifficulty: { width: "18%" },
    colInnerAttempts: { width: "15%" },
    colInnerSuccesses: { width: "15%" },
    colInnerFailures: { width: "15%" },
    colInnerRate: { width: "15%" },

    "@media (max-width: 700px)": {
      colInnerGameType: { width: "220px" },
      colInnerDifficulty: { width: "180px" },
      colInnerAttempts: { width: "140px" },
      colInnerSuccesses: { width: "140px" },
      colInnerFailures: { width: "140px" },
      colInnerRate: { width: "140px" },
    },

    innerRow: {
      "&:hover": {
        background: color.bg,
      },
    },

    colGameType: { textAlign: "center" },
    colDifficulty: { textAlign: "center" },
    colAttempts: { textAlign: "center" },
    colRate: { textAlign: "center" },
  })();
};
