import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  container: {
    display: "flex",
    flexDirection: "column",
    height: "628px",
    width: "100%",
    maxWidth: "800px",

    border: "1px solid #e1e5e9",
    borderRadius: "12px",
    backgroundColor: "#ffffff",
    overflow: "hidden",
    position: "relative",
    fontFamily:
      "-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif",
  },

  header: {
    flexShrink: 0,
  },

  messageArea: {
    flex: 1,
    display: "flex",
    flexDirection: "column",
    minHeight: 0,
  },

  messageList: {
    flex: 1,
    minHeight: 0,
  },

  inputArea: {
    flexShrink: 0,
    borderTop: "1px solid #e1e5e9",
    backgroundColor: "#f8f9fa",
  },

  errorContainer: {
    padding: "12px 16px",
    backgroundColor: "#fee",
    borderBottom: "1px solid #fcc",
    color: "#c33",
    fontSize: "14px",
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
  },

  errorMessage: {
    flex: 1,
  },

  errorDismiss: {
    background: "none",
    border: "none",
    color: "#c33",
    cursor: "pointer",
    padding: "4px 8px",
    borderRadius: "4px",
    fontSize: "12px",
    fontWeight: "500",

    "&:hover": {
      backgroundColor: "#fdd",
    },
  },

  demoIndicator: {
    padding: "8px 16px",
    backgroundColor: "#e3f2fd",
    borderBottom: "1px solid #bbdefb",
    color: "#1565c0",
    fontSize: "13px",
    fontWeight: "500",
    textAlign: "center",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    gap: "8px",
  },

  loadingOverlay: {
    position: "absolute",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: "rgba(255, 255, 255, 0.8)",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    zIndex: 10,
  },

  loadingSpinner: {
    width: "24px",
    height: "24px",
    border: "2px solid #e1e5e9",
    borderTop: "2px solid #007bff",
    borderRadius: "50%",
    animation: "$spin 1s linear infinite",
  },

  "@keyframes spin": {
    "0%": { transform: "rotate(0deg)" },
    "100%": { transform: "rotate(360deg)" },
  },

  "@media (max-width: 768px)": {
    container: {
      height: "calc(100vh - 40px)",
      maxWidth: "100%",
      borderRadius: "8px",
      margin: "20px",
    },
  },

  "@media (max-width: 480px)": {
    container: {
      fontSize: "14px",
    },
  },
});
