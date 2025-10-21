import { createUseStyles } from "react-jss";
import { Theme } from "@mui/material/styles";

export const useStyles = createUseStyles((theme: Theme) => ({
  tableRow: {
    "&:hover": {
      backgroundColor:
        theme.palette.mode === "dark"
          ? theme.palette.grey[800]
          : theme.palette.grey[50],
    },
    "& .MuiTableCell-root": {
      padding: "12px 16px",
      borderBottom: `1px solid ${theme.palette.divider}`,
      color: theme.palette.text.primary,
    },
  },
  userInfo: {
    display: "flex",
    alignItems: "center",
  },
  avatar: {
    width: "32px",
    height: "32px",
    fontSize: "14px",
    marginRight: "12px",
  },
  userName: {
    fontWeight: "500",
    fontSize: "14px",
    color: theme.palette.text.primary,
  },
  roleChip: {
    height: "24px",
    fontSize: "12px",
    textTransform: "capitalize",
    color: "white",
    fontWeight: "500",
  },
  onlineChip: {
    height: "24px",
    fontSize: "12px",
  },
  connectionCount: {
    fontSize: "14px",
    color: theme.palette.text.secondary,
  },
  // Mobile card styles
  mobileCard: {
    marginBottom: "12px",
    borderRadius: "8px",
    border: `1px solid ${theme.palette.divider}`,
    backgroundColor: theme.palette.background.paper,
  },
  mobileCardContent: {
    padding: "16px !important",
    "&:last-child": {
      paddingBottom: "16px !important",
    },
  },
  mobileUserHeader: {
    display: "flex",
    alignItems: "center",
    marginBottom: "12px",
  },
  mobileUserInfo: {
    flex: 1,
    marginLeft: "12px",
  },
  mobileUserName: {
    fontWeight: "500",
    fontSize: "16px",
    marginBottom: "4px",
    color: theme.palette.text.primary,
  },
  mobileConnectionInfo: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    marginTop: "12px",
    paddingTop: "12px",
    borderTop: `1px solid ${theme.palette.divider}`,
  },
}));
