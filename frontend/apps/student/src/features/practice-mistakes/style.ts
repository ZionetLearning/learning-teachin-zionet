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
    zIndex: 10,
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
  retryButton: {
    textTransform: "none",
    borderRadius: 999,
    px: 1.8,
    py: 0.6,
    bgcolor: "#7c4dff",
    "&:hover": { bgcolor: "#6f3cff" },
  },
});
