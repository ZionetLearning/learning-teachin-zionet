import { createUseStyles } from "react-jss";
import { useThemeColors } from "@app-providers";

export const useStyles = () => {
  const color = useThemeColors();

  return createUseStyles({
    listContainer: {
      display: "flex",
      flexDirection: "column",
      flex: 1,
      backgroundColor: color.paper,
      borderRadius: "12px",
      boxShadow: "0 8px 32px rgba(0, 0, 0, 0.1)",
      border: `1px solid ${color.divider}`,
      backdropFilter: "blur(20px)",
      padding: "2rem",
      minHeight: "600px",
    },
    sectionTitle: {
      fontSize: "1.5rem",
      fontWeight: "600",
      color: color.text,
      marginBottom: "1.5rem",
      borderBottom: `2px solid rgba(var(${color.primaryMainChannel}) / 1)`,
      paddingBottom: "0.5rem",
    },
    tableArea: {
      flex: 1,
      display: "flex",
      flexDirection: "column",
    },
    tableShell: {
      flex: 1,
      border: `1px solid ${color.divider}`,
      borderRadius: "8px",
      overflow: "hidden",
      display: "flex",
      flexDirection: "column",
    },
    headerTable: {
      backgroundColor: color.bg,
      "& .MuiTableCell-head": {
        fontWeight: "600",
        color: color.text,
        borderBottom: `2px solid ${color.divider}`,
      },
    },
    searchBar: {
      padding: "1rem",
      backgroundColor: color.bg,
      borderBottom: `1px solid ${color.divider}`,
    },
    searchField: {
      backgroundColor: color.paper,
      "& .MuiOutlinedInput-root": {
        borderRadius: "8px",
      },
    },
    rowsScroll: {
      flex: 1,
      overflowY: "auto",
      maxHeight: "400px",
    },
    bodyTable: {
      "& .MuiTableCell-root": {
        borderBottom: `1px solid ${color.divider}`,
        padding: "12px 16px",
      },
    },
    tableRow: {
      cursor: "pointer",
      transition: "background-color 0.2s ease",
      "&:hover": {
        backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.08)`,
      },
      "&:nth-child(even)": {
        backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.03)`,
      },
      "&:nth-child(even):hover": {
        backgroundColor: `rgba(var(${color.primaryMainChannel}) / 0.08)`,
      },
    },
    taskName: {
      fontWeight: "500",
      color: color.text,
      overflow: "hidden",
      textOverflow: "ellipsis",
    },
    taskPayload: {
      color: color.textMuted,
      fontSize: "0.9rem",
      overflow: "hidden",
      textOverflow: "ellipsis",
      fontFamily: "monospace",
      backgroundColor: color.bg,
      padding: "4px 8px",
      borderRadius: "4px",
      border: `1px solid ${color.divider}`,
    },
    actionsContainer: {
      display: "flex",
      gap: "4px",
      justifyContent: "center",
      alignItems: "center",
    },
    paginationBar: {
      borderTop: `1px solid ${color.divider}`,
      backgroundColor: color.bg,
      "& .MuiTablePagination-toolbar": {
        paddingLeft: "1rem",
        paddingRight: "1rem",
      },
    },
  })();
};
