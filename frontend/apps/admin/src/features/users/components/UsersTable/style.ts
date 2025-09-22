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
  },
  tableShell: {
    flex: 1,
    display: "flex",
    flexDirection: "column",
    minHeight: 0,
    border: "1px solid #e5e7eb",
    borderRadius: 8,
    background: "#fff",
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
  searchBar: {
    padding: "8px 12px 6px",
    borderBottom: "1px solid #e5e7eb",
    background: "#fff",
  },
  searchField: {
    "& .MuiOutlinedInput-root": {
      height: 32,
      borderRadius: 8,
      background: "#f8fafc",
    },
    "& .MuiOutlinedInput-input": {
      padding: "4px 8px",
      fontSize: 13,
      lineHeight: 1.2,
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
    overflowY: "auto",
    scrollbarGutter: "stable",
  },
  bodyTable: {
    tableLayout: "fixed",
    width: "100%",
    borderCollapse: "separate",
    borderSpacing: 0,
    "& td": {
      borderBottom: "1px solid #f1f5f9",
    },
  },
  paginationBar: {
    padding: "4px 12px",
    borderTop: "1px solid #e5e7eb",
    background: "#fff",
  },
  tableScrollX: {
    overflowX: "auto",
    WebkitOverflowScrolling: "touch",
    "&::-webkit-scrollbar:horizontal": { height: 8 },
    "&::-webkit-scrollbar-thumb": { borderRadius: 8 },
    "& table": {
      minWidth: 720, // desktop/tablet baseline
      "@media (max-width: 900px)": { minWidth: 640 },
      "@media (max-width: 600px)": { minWidth: 560 },
    },
  },
});
