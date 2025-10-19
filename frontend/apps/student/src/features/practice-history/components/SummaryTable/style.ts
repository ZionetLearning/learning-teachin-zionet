import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  tableWrapper: {
    width: "100%",
    display: "flex",
    justifyContent: "center",
    padding: 16,
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
});
