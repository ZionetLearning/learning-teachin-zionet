import { createUseStyles } from "react-jss";
import { Theme } from "@mui/material/styles";

export const useStyles = createUseStyles((theme: Theme) => ({
  container: {
    padding: "16px",
    maxWidth: "1200px",
    margin: "0 auto",
    display: "flex",
    flexDirection: "column",
    overflow: "hidden",
    "@media (max-width: 600px)": {
      height: "calc(100vh - 25px)",
      padding: "12px",
    },
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
  emptyState: {
    textAlign: "center",
    padding: "60px 20px",
  },
  emptyIcon: {
    fontSize: "48px",
    marginBottom: "16px",
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
  contentArea: {
    flex: 1,
    overflow: "hidden",
    display: "flex",
    flexDirection: "column",
  },
  scrollableContainer: {
    flex: 1,
    overflow: "auto",
    paddingInlineEnd: "4px",
    paddingBottom: "16px",
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
  mobileCardList: {
    display: "none",
    "@media (max-width: 600px)": {
      display: "block",
      flex: 1,
      overflow: "auto",
      paddingInlineEnd: "4px",
      paddingBottom: "24px",
      "&::-webkit-scrollbar": {
        width: "6px",
      },
      "&::-webkit-scrollbar-track": {
        backgroundColor:
          theme.palette.mode === "dark"
            ? theme.palette.grey[800]
            : theme.palette.grey[200],
        borderRadius: "3px",
      },
      "&::-webkit-scrollbar-thumb": {
        backgroundColor:
          theme.palette.mode === "dark"
            ? theme.palette.grey[600]
            : theme.palette.grey[400],
        borderRadius: "3px",
        "&:hover": {
          backgroundColor:
            theme.palette.mode === "dark"
              ? theme.palette.grey[500]
              : theme.palette.grey[500],
        },
      },
    },
  },
}));
