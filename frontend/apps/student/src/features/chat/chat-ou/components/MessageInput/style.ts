import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  container: {
    padding: "16px",
    borderTop: "1px solid #e1e5e9",
    backgroundColor: "#ffffff",
    position: "sticky",
    bottom: 0,
    zIndex: 10,
  },

  contextDisplay: {
    backgroundColor: "#f8f9fa",
    border: "1px solid #e1e5e9",
    borderRadius: "12px",
    padding: "12px",
    marginBottom: "8px",
  },

  contextHeader: {
    display: "flex",
    alignItems: "center",
    gap: "8px",
    marginBottom: "8px",
  },

  contextIcon: {
    fontSize: "14px",
  },

  contextLabel: {
    fontSize: "12px",
    fontWeight: "600",
    color: "#495057",
    flex: 1,
  },

  contextActions: {
    display: "flex",
    gap: "4px",
  },

  contextActionButton: {
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    width: "24px",
    height: "24px",
    border: "none",
    backgroundColor: "transparent",
    borderRadius: "4px",
    cursor: "pointer",
    fontSize: "12px",
    color: "#6c757d",
    transition: "all 0.2s ease",

    "&:hover": {
      backgroundColor: "#e9ecef",
      color: "#495057",
    },

    "&:active": {
      transform: "scale(0.95)",
    },
  },

  contextContent: {
    fontSize: "12px",
    color: "#6c757d",
    lineHeight: "16px",
    whiteSpace: "pre-line",
    maxHeight: "60px",
    overflow: "auto",
    padding: "4px 0",
  },

  inputWrapper: {
    display: "flex",
    alignItems: "flex-end",
    gap: "8px",
    backgroundColor: "#f8f9fa",
    borderRadius: "24px",
    padding: "8px 12px",
    border: "1px solid #e1e5e9",
    transition: "border-color 0.2s ease",

    "&:focus-within": {
      borderColor: "#007bff",
      boxShadow: "0 0 0 2px rgba(0, 123, 255, 0.1)",
    },
  },

  contextButton: {
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    width: "32px",
    height: "32px",
    border: "none",
    backgroundColor: "transparent",
    borderRadius: "50%",
    cursor: "pointer",
    fontSize: "16px",
    color: "#6c757d",
    transition: "all 0.2s ease",
    flexShrink: 0,

    "&:hover:not(:disabled)": {
      backgroundColor: "#e9ecef",
      color: "#495057",
    },

    "&:disabled": {
      cursor: "not-allowed",
      opacity: 0.5,
    },

    "&:focus": {
      outline: "none",
      boxShadow: "0 0 0 2px rgba(0, 123, 255, 0.2)",
    },
  },

  contextButtonActive: {
    backgroundColor: "#007bff",
    color: "white",

    "&:hover:not(:disabled)": {
      backgroundColor: "#0056b3",
      color: "white",
    },
  },

  textarea: {
    flex: 1,
    border: "none",
    outline: "none",
    backgroundColor: "transparent",
    resize: "none",
    fontSize: "14px",
    lineHeight: "20px",
    padding: "8px 0",
    minHeight: "20px",
    maxHeight: "120px",
    overflow: "auto",
    fontFamily: "inherit",
    color: "#333333",

    "&::placeholder": {
      color: "#6c757d",
    },

    "&:disabled": {
      color: "#6c757d",
      cursor: "not-allowed",
    },
  },

  sendButton: {
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    width: "36px",
    height: "36px",
    borderRadius: "50%",
    border: "none",
    backgroundColor: "#007bff",
    color: "white",
    cursor: "pointer",
    transition: "all 0.2s ease",
    flexShrink: 0,

    "&:hover:not(:disabled)": {
      backgroundColor: "#0056b3",
      transform: "scale(1.05)",
    },

    "&:active:not(:disabled)": {
      transform: "scale(0.95)",
    },

    "&:focus": {
      outline: "none",
      boxShadow: "0 0 0 2px rgba(0, 123, 255, 0.3)",
    },
  },

  sendButtonDisabled: {
    backgroundColor: "#6c757d",
    cursor: "not-allowed",
    transform: "none",

    "&:hover": {
      backgroundColor: "#6c757d",
      transform: "none",
    },
  },

  sendIcon: {
    fontSize: "16px",
    lineHeight: 1,
    transform: "translateX(1px)",
  },

  loadingSpinner: {
    fontSize: "14px",
    animation: "$spin 1s linear infinite",
  },

  characterCount: {
    fontSize: "12px",
    color: "#6c757d",
    textAlign: "right",
    marginTop: "4px",
    paddingRight: "8px",
  },

  suggestionsPanel: {
    position: "absolute",
    bottom: "100%",
    left: "16px",
    right: "16px",
    backgroundColor: "#ffffff",
    border: "1px solid #e1e5e9",
    borderRadius: "12px",
    boxShadow: "0 4px 12px rgba(0, 0, 0, 0.1)",
    marginBottom: "8px",
    zIndex: 20,
  },

  suggestionsHeader: {
    padding: "12px 16px 8px",
    fontSize: "13px",
    fontWeight: "600",
    color: "#495057",
    borderBottom: "1px solid #f1f3f4",
  },

  suggestionsList: {
    padding: "8px",
    display: "flex",
    flexDirection: "column",
    gap: "4px",
  },

  suggestionButton: {
    padding: "8px 12px",
    border: "none",
    backgroundColor: "transparent",
    borderRadius: "8px",
    cursor: "pointer",
    fontSize: "14px",
    color: "#495057",
    textAlign: "left",
    transition: "all 0.2s ease",

    "&:hover": {
      backgroundColor: "#f8f9fa",
      color: "#007bff",
    },

    "&:active": {
      backgroundColor: "#e9ecef",
    },
  },

  "@keyframes spin": {
    "0%": { transform: "rotate(0deg)" },
    "100%": { transform: "rotate(360deg)" },
  },
});
