import { createUseStyles } from "react-jss";
import { Theme } from "@mui/material/styles";

export const useStyles = createUseStyles((theme: Theme) => ({
  container: {
    padding: "16px",
    maxWidth: "1200px",
    margin: "0 auto",
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
    color: theme.palette.text.secondary,
    opacity: theme.palette.mode === "dark" ? 0.9 : 0.8,
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
  mobileCardList: {
    display: "none",
    "@media (max-width: 600px)": {
      display: "block",
    },
  },
}));
