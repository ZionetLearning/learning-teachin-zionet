import { createUseStyles } from "react-jss";
import { Theme } from "@mui/material/styles";

export const useStyles = createUseStyles((theme: Theme) => ({
  container: {
    padding: "16px",
    maxWidth: "1200px",
    margin: "0 auto",
  },
  header: {
    marginBottom: "24px",
  },
  title: {
    marginBottom: "8px",
  },
  subtitle: {
    marginBottom: "16px",
  },
  statusChip: {
    marginBottom: "24px",
  },
  statusDot: {
    width: "8px",
    height: "8px",
    borderRadius: "50%",
  },
  statusDotConnected: {
    backgroundColor: theme.palette.success.main,
  },
  statusDotDisconnected: {
    backgroundColor: theme.palette.warning.main,
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
  emptyState: {
    textAlign: "center",
    padding: "60px 20px",
  },
  emptyIcon: {
    fontSize: "48px",
    marginBottom: "16px",
  },
  loadingContainer: {
    display: "flex",
    flexDirection: "column",
    alignItems: "center",
    justifyContent: "center",
    minHeight: "200px",
    gap: "16px",
  },
  errorContainer: {
    padding: "20px",
    textAlign: "center",
  },
  // Mobile responsive styles
  desktopTable: {
    "@media (max-width: 600px)": {
      display: "none",
    },
  },
  mobileCardList: {
    display: "none",
    "@media (max-width: 600px)": {
      display: "block",
    },
  },
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
