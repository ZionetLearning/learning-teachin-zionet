import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  listContainer: {
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
    display: "flex",
    flexDirection: "column",
    minHeight: 0,
    flex: "1 1 60%",
    height: "calc(100vh - 3rem)",
    "@supports (height: 100dvh)": {
      height: "calc(100dvh - 3rem)",
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
  filtersRow: {
    margin: "0 0 12px",
    display: "flex",
    gap: 12,
    "@media (max-width: 600px)": {
      flexDirection: "column !important",
      gap: 8,
    },
  },
  filterControl: {
    minWidth: 180,
    "@media (max-width: 600px)": {
      minWidth: "100%",
      width: "100%",
    },
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
      textAlign: "center",
      background: "#f8fafc",
      fontWeight: 600,
      color: "#1f2937",
      borderBottom: "1px solid #e5e7eb",
      whiteSpace: "nowrap",
      position: "sticky",
      top: 0,
      zIndex: 1,
    },
    "& .MuiTableBody-root .MuiTableCell-root": {
      textAlign: "center",
      borderBottom: "1px solid #f1f5f9",
      whiteSpace: "nowrap",
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
      minWidth: 980,
    },
  },
  cap: { textTransform: "capitalize" },
  colStudent: {
    minWidth: 180,
  },
  ellipsis: {
    display: "block",
    maxWidth: "100%",
    overflow: "hidden",
    textOverflow: "ellipsis",
    whiteSpace: "nowrap",
  },
  colGameType: { minWidth: 120 },
  colDifficulty: { minWidth: 120 },
  colAttempts: { minWidth: 110, textAlign: "center" },
  colSuccesses: { minWidth: 110, textAlign: "center" },
  colFailures: { minWidth: 110, textAlign: "center" },
  colRate: { minWidth: 160 },
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
    color: "#475569",
  },
  paginationBar: {
    padding: "4px 12px",
    borderTop: "1px solid #e5e7eb",
    background: "#fff",
  },
});
