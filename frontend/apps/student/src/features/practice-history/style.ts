import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  headerWrapper: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    textAlign: "center",
    background:
      "linear-gradient(180deg, rgba(124,77,255,0.10) 0%, rgba(124,77,255,0.03) 100%)",
  },
  tableWrapper: {
    width: "100%",
    display: "flex",
    justifyContent: "center",
    padding: 16,
  },
  title: {
    color: "#7c4dff",
    fontSize: 26,
    fontWeight: 700,
    letterSpacing: 0.2,
  },
  description: {
    color: "#7c4dff",
    fontSize: 16,
    opacity: 0.9,
  },
  // table header cells
  th: {
    fontWeight: 700,
    textAlign: "center",
    backdropFilter: "blur(4px)",
    backgroundColor: "rgba(255,255,255,0.8) !important",
  },
  // table body cells
  td: {
    textAlign: "center",
    "&:global(.MuiTableRow-root:nth-of-type(odd)) &": {
      backgroundColor: "rgba(0,0,0,0.015)",
    },
  },
  tablePaginationWrapper: {
    position: "fixed",
    bottom: 0,
    left: 0,
    right: 0,
    background: "rgba(255,255,255,0.98)",
    borderTop: "1px solid #eaeaea",
    backdropFilter: "blur(6px)",
  },
  paperWrapper: {
    width: "100%",
    maxWidth: 1100,
    borderRadius: 3,
    overflow: "hidden",
  },
  tableContainer: {
    maxHeight: "min(95vh, 700px)",
  },
  lastAnswerBox: {
    maxWidth: 420,
    mx: "auto",
    whiteSpace: "nowrap",
    overflow: "hidden",
    textOverflow: "ellipsis",
  },
  toggleGroupWrapper: {
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    gap: 12,
    marginTop: 16,
    backgroundColor: "rgba(255,255,255,0.15)", // fallback for light mode
    borderRadius: 50,
    padding: 6,
    backdropFilter: "blur(6px)",
    "@media (prefers-color-scheme: dark)": {
      backgroundColor: "rgba(0,0,0,0.4)",
    },
  },
  toggleGroup: {
    "& .MuiToggleButton-root": {
      color: "#7c4dff",
      margin: "0 6px",
      border: "none",
      textTransform: "none",
      fontWeight: 600,
      borderRadius: 999,
      transition: "all 0.25s ease",
      padding: "6px 20px",
      backgroundColor: "rgba(124,77,255,0.08)",
      "&:hover": {
        backgroundColor: "rgba(124,77,255,0.2)",
      },
      "@media (prefers-color-scheme: dark)": {
        color: "#d1b3ff",
        backgroundColor: "rgba(124,77,255,0.15)",
        "&:hover": {
          backgroundColor: "rgba(124,77,255,0.3)",
        },
      },
    },
    "& .Mui-selected": {
      color: "#fff !important",
      backgroundColor: "#7c4dff !important",
      boxShadow: "0 2px 8px rgba(124,77,255,0.4)",
      "&:hover": {
        backgroundColor: "#6f3cff !important",
        boxShadow: "0 2px 10px rgba(124,77,255,0.6)",
      },
    },
  },
});
