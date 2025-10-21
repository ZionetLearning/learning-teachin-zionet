import { createUseStyles } from "react-jss";
import { Theme } from "@mui/material/styles";

export const useStyles = createUseStyles((theme: Theme) => ({
  desktopTable: {
    "@media (max-width: 600px)": {
      display: "none",
    },
  },
  tableContainer: {
    borderRadius: "8px",
    border: `1px solid ${theme.palette.divider}`,
    overflow: "hidden",
    backgroundColor: theme.palette.background.paper,
  },
  tableHead: {
    backgroundColor:
      theme.palette.mode === "dark"
        ? theme.palette.grey[800]
        : theme.palette.grey[100],
    "& .MuiTableCell-head": {
      fontWeight: "600",
      fontSize: "14px",
      padding: "12px 16px",
      color: theme.palette.text.primary,
    },
  },
}));
