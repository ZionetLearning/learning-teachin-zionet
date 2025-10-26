import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  cardBase: {
    background: "#fff",
    border: "1px solid #e2e8f0",
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
    color: "#1a202c",
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
    display: "flex",
    flexDirection: "column",
    minHeight: 0,
    border: "1px solid #e5e7eb",
    borderRadius: 8,
    background: "#fff",
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
      background: "#f8fafc",
      fontWeight: 600,
      color: "#1f2937",
      borderBottom: "1px solid #e5e7eb",
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
    borderBottom: "1px solid #e5e7eb",
    background: "#fff",
    "@media (max-width: 480px)": {
      padding: "10px 8px 8px",
    },
  },
  searchField: {
    "& .MuiOutlinedInput-root": {
      height: 32,
      borderRadius: 8,
      background: "#f8fafc",
      "@media (max-width: 480px)": {
        height: 36,
      },
    },
    "& .MuiOutlinedInput-input": {
      padding: "4px 8px",
      fontSize: 13,
      lineHeight: 1.2,
      "@media (max-width: 480px)": {
        fontSize: 14,
        padding: "6px 10px",
      },
    },
    "& input:-webkit-autofill, & input:-webkit-autofill:hover, & input:-webkit-autofill:focus":
      {
        WebkitBoxShadow: "0 0 0 1000px #f8fafc inset",
        boxShadow: "0 0 0 1000px #f8fafc inset",
        WebkitTextFillColor: "#1f2937",
        caretColor: "#1f2937",
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
      borderBottom: "1px solid #f1f5f9",
    },
  },
  paginationBar: {
    padding: "8px 16px",
    borderTop: "1px solid #d1d5db",
    background: "#f9fafb",
    flex: "0 0 auto",
    marginTop: "auto",
    borderBottomLeftRadius: 8,
    borderBottomRightRadius: 8,

    "& .MuiTablePagination-toolbar": {
      minHeight: 44,
      "@media (max-width: 480px)": {
        minHeight: 36,
        flexWrap: "wrap",
        gap: "8px",
      },
    },

    "& .MuiTablePagination-selectLabel, & .MuiTablePagination-displayedRows": {
      fontSize: 14,
      fontWeight: 500,
      color: "#374151",
      "@media (max-width: 480px)": {
        fontSize: 12,
      },
    },

    "& .MuiTablePagination-select": {
      fontSize: 14,
      fontWeight: 600,
      backgroundColor: "#fff",
      border: "1px solid #d1d5db",
      borderRadius: 6,
      padding: "4px 28px 4px 8px",
      marginLeft: 8,
      "&:hover": {
        borderColor: "#9ca3af",
      },
      "@media (max-width: 480px)": {
        fontSize: 12,
        padding: "2px 24px 2px 6px",
        marginLeft: 4,
      },
    },

    "& .MuiTablePagination-actions .MuiIconButton-root": {
      backgroundColor: "#fff",
      border: "1px solid #d1d5db",
      borderRadius: 6,
      margin: "0 1px",
      transition: "all 0.15s ease",
      "&:hover": {
        backgroundColor: "#3b82f6",
        borderColor: "#3b82f6",
        color: "#fff",
      },
      "&.Mui-disabled": {
        backgroundColor: "#f3f4f6",
        borderColor: "#e5e7eb",
        color: "#9ca3af",
      },
      "@media (max-width: 480px)": {
        padding: "4px",
        margin: "0 0.5px",
      },
    },

    "@media (max-width: 480px)": {
      padding: "6px 12px",
    },
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
      minWidth: 720, // desktop/tablet baseline
      "@media (max-width: 900px)": { minWidth: 640 },
      "@media (max-width: 600px)": { minWidth: 480 },
      "@media (max-width: 480px)": { minWidth: 400 },
    },
  },
});
