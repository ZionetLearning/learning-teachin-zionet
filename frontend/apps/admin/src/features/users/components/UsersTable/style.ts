import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    cardBase: {
      background: color.paper,
      border: `1px solid ${color.divider}`,
      boxShadow: "0 2px 4px rgba(0,0,0,0.04), 0 4px 12px rgba(0,0,0,0.03)",
      borderRadius: 12,
      padding: "1.25rem 1.15rem 1.5rem",
      position: "relative",
      transition: "box-shadow .25s ease, transform .25s ease",
      "&:hover": {
        boxShadow: "0 4px 10px rgba(0,0,0,0.06), 0 6px 18px rgba(0,0,0,0.05)",
        transform: "translateY(-2px)",
      },
    },
    listContainer: {
      composes: "$cardBase",
      flex: "1 1 60%",
      display: "flex",
      flexDirection: "column",
      minHeight: 0,
      alignSelf: "center",
      "@media (max-width: 768px)": {
        flex: "0 0 auto",
        minHeight: "auto",
        alignSelf: "stretch",
      },
      "@media (max-width: 480px)": {
        padding: "1rem 0.75rem 1.25rem",
      },
    },
    sectionTitle: {
      margin: "0 0 0.85rem",
      fontSize: 18,
      fontWeight: 600,
      letterSpacing: 0.3,
      color: color.text,
      textAlign: "center",
    },
    tableArea: {
      flex: 1,
      display: "flex",
      flexDirection: "column",
      minHeight: 0,
      "@media (max-width: 768px)": {
        flex: "0 0 auto",
      },
    },
    tableShell: {
      flex: 1,
      display: "flex",
      flexDirection: "column",
      minHeight: 0,
      border: `1px solid ${color.divider}`,
      borderRadius: 8,
      background: color.paper,
      "@media (max-width: 480px)": {
        borderRadius: 6,
      },
    },
    headerTable: {
      tableLayout: "fixed",
      width: "100%",
      borderCollapse: "separate",
      borderSpacing: 0,
      "& th": {
        background: `rgba(var(${color.primaryMainChannel}) / 0.06)`,
        fontWeight: 600,
        color: color.text,
        borderBottom: `1px solid ${color.divider}`,
      },
    },
    headerCell: {
      "@media (max-width: 768px)": {
        fontSize: 13,
        padding: "6px 4px",
      },
      "@media (max-width: 480px)": {
        fontSize: 11,
        padding: "4px 2px",
      },
    },
    searchBar: {
      padding: "8px 12px 6px",
      borderBottom: `1px solid ${color.divider}`,
      background: color.paper,
      "@media (max-width: 480px)": {
        padding: "10px 8px 8px",
      },
    },
    searchField: {
      "& .MuiOutlinedInput-root": {
        height: 32,
        borderRadius: 8,
        background: `rgba(var(${color.primaryMainChannel}) / 0.05)`,
        "@media (max-width: 480px)": {
          height: 36,
        },
      },
      "& .MuiOutlinedInput-input": {
        padding: "4px 8px",
        fontSize: 13,
        lineHeight: 1.2,
        color: color.text,
        "@media (max-width: 480px)": {
          fontSize: 14,
          padding: "6px 10px",
        },
      },
      "& input:-webkit-autofill, & input:-webkit-autofill:hover, & input:-webkit-autofill:focus":
        {
          WebkitBoxShadow: `0 0 0 1000px ${color.paper} inset`,
          boxShadow: `0 0 0 1000px ${color.paper} inset`,
          WebkitTextFillColor: color.text,
          caretColor: color.text,
          transition: "background-color 9999s ease-out, color 9999s ease-out",
        },
    },
    rowsScroll: {
      flex: 1,
      minHeight: 0,
      maxHeight: "calc(100vh - 300px)",
      overflowY: "auto",
      scrollbarGutter: "stable",
      display: "flex",
      flexDirection: "column",
      "@media (max-width: 768px)": {
        maxHeight: "none",
        flex: "0 0 auto",
      },
      "@media (max-width: 480px)": {
        maxHeight: "none",
      },
    },
    bodyTable: {
      tableLayout: "fixed",
      width: "100%",
      borderCollapse: "separate",
      borderSpacing: 0,
      flex: "0 0 auto",
      "& td": {
        borderBottom: `1px solid ${color.divider}`,
      },
    },
    paginationBar: {
      background: color.paper,
      borderTop: `1px solid ${color.divider}`,
      backdropFilter: "blur(6px)",
      boxShadow: "0 -4px 12px rgba(0,0,0,0.04), 0 -12px 32px rgba(0,0,0,0.03)",
      position: "relative",
      zIndex: 2,
    },
    tableContainer: {
      flex: "1 1 auto",
      overflowY: "auto",
    },
    tableScrollX: {
      overflowX: "auto",
      WebkitOverflowScrolling: "touch",
      "&::-webkit-scrollbar:horizontal": { height: 8 },
      "&::-webkit-scrollbar-thumb": { borderRadius: 8 },
      "& table": {
        minWidth: 720,
        "@media (max-width: 900px)": { minWidth: 640 },
        "@media (max-width: 600px)": { minWidth: 480 },
        "@media (max-width: 480px)": { minWidth: 400 },
      },
    },
  })();
};
