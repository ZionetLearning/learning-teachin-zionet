import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  listContainer: {
    display: "flex",
    flexDirection: "column",
    flex: 1,
    backgroundColor: "#ffffff",
    borderRadius: "12px",
    boxShadow: "0 8px 32px rgba(0, 0, 0, 0.1)",
    border: "1px solid rgba(255, 255, 255, 0.2)",
    backdropFilter: "blur(20px)",
    padding: "2rem",
    minHeight: "600px",
  },
  sectionTitle: {
    fontSize: "1.5rem",
    fontWeight: "600",
    color: "#2c3e50",
    marginBottom: "1.5rem",
    borderBottom: "2px solid #3498db",
    paddingBottom: "0.5rem",
  },
  tableArea: {
    flex: 1,
    display: "flex",
    flexDirection: "column",
  },
  tableShell: {
    flex: 1,
    border: "1px solid #e0e0e0",
    borderRadius: "8px",
    overflow: "hidden",
    display: "flex",
    flexDirection: "column",
  },
  headerTable: {
    backgroundColor: "#f8f9fa",
    "& .MuiTableCell-head": {
      fontWeight: "600",
      color: "#2c3e50",
      borderBottom: "2px solid #dee2e6",
    },
  },
  searchBar: {
    padding: "1rem",
    backgroundColor: "#f8f9fa",
    borderBottom: "1px solid #e0e0e0",
  },
  searchField: {
    backgroundColor: "#ffffff",
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
      borderBottom: "1px solid #f0f0f0",
      padding: "12px 16px",
    },
  },
  tableRow: {
    cursor: 'pointer',
    transition: 'background-color 0.2s ease',
    "&:hover": {
      backgroundColor: "#e3f2fd",
    },
    "&:nth-child(even)": {
      backgroundColor: "#fafafa",
    },
    "&:nth-child(even):hover": {
      backgroundColor: "#e3f2fd",
    },
  },
  taskName: {
    fontWeight: "500",
    color: "#2c3e50",
    overflow: "hidden",
    textOverflow: "ellipsis",
  },
  taskPayload: {
    color: "#6c757d",
    fontSize: "0.9rem",
    overflow: "hidden",
    textOverflow: "ellipsis",
    fontFamily: "monospace",
    backgroundColor: "#f8f9fa",
    padding: "4px 8px",
    borderRadius: "4px",
    border: "1px solid #e9ecef",
  },
  actionsContainer: {
    display: "flex",
    gap: "4px",
    justifyContent: "center",
    alignItems: "center",
  },
  paginationBar: {
    borderTop: "1px solid #e0e0e0",
    backgroundColor: "#f8f9fa",
    "& .MuiTablePagination-toolbar": {
      paddingLeft: "1rem",
      paddingRight: "1rem",
    },
  },
});