import { createUseStyles } from "react-jss";
import { Theme } from "@mui/material/styles";

export const useStyles = createUseStyles((theme: Theme) => ({
  desktopTable: {
    flex: 1,
    display: "flex",
    flexDirection: "column",
    overflow: "hidden",
    "@media (max-width: 600px)": {
      display: "none",
    },
  },
  tableContainer: {
    borderRadius: "8px",
    border: `1px solid ${theme.palette.divider}`,
    backgroundColor: theme.palette.background.paper,
    overflow: "auto",
    maxHeight: "calc(100vh - 200px)",
    "&::-webkit-scrollbar": {
      width: "8px",
    },
    "&::-webkit-scrollbar-track": {
      backgroundColor:
        theme.palette.mode === "dark"
          ? theme.palette.grey[800]
          : theme.palette.grey[200],
      borderRadius: "4px",
    },
    "&::-webkit-scrollbar-thumb": {
      backgroundColor:
        theme.palette.mode === "dark"
          ? theme.palette.grey[600]
          : theme.palette.grey[400],
      borderRadius: "4px",
      "&:hover": {
        backgroundColor:
          theme.palette.mode === "dark"
            ? theme.palette.grey[500]
            : theme.palette.grey[500],
      },
    },
  },
  tableHead: {
    backgroundColor:
      theme.palette.mode === "dark"
        ? theme.palette.grey[800]
        : theme.palette.grey[100],
    position: "sticky",
    top: 0,
    zIndex: 10,
    "& .MuiTableCell-head": {
      fontWeight: "600",
      fontSize: "14px",
      padding: "12px 16px",
      color: theme.palette.text.primary,
      backgroundColor:
        theme.palette.mode === "dark"
          ? theme.palette.grey[800]
          : theme.palette.grey[100],
    },
  },
}));
